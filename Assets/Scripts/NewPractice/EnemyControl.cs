using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

// 체스 기물 규칙을 따르며 자동으로 이동하는 NPC 컨트롤러
// 최적화를 위해 코루틴과 타이머를 사용하여 너무 자주 업데이트하지 않음
public class EnemyControl : MonoBehaviour
{
    [Header("맵 및 컴포넌트 연동")]
    [SerializeField] private MapScanner mapScanner;
    [SerializeField] private Grid grid;
    [SerializeField] private ChessPiece chessPiece;

    [Header("이동 설정")]
    [SerializeField] private float moveInterval = 2f; // 이동 간격 (초)
    [SerializeField] private float moveSpeed = 5f; // 한 칸 이동 속도

    [Header("AI 동작 모드")]
    [SerializeField] private AIMode aiMode = AIMode.Random;

    private Vector3Int currentCell;
    private Vector3Int targetCell;
    private bool isMoving = false;
    private float moveTimer;

    // 렉 방지를 위해 각 Enemy의 업데이트 타이밍을 랜덤으로 오프셋
    private float randomOffset;

    // 캐싱: 이동 가능한 칸 목록을 저장하여 매번 계산하지 않음
    private List<Vector3Int> cachedPossibleMoves = new List<Vector3Int>();
    private float cacheUpdateTime = 0f;
    private const float CACHE_UPDATE_INTERVAL = 0.5f; // 캐시 갱신 간격

    public enum AIMode
    {
        Random,      // 랜덤 이동
        Patrol,      // 제자리에서 가만히 (패트롤 확장용)
        Chase        // 플레이어 추적 (확장용)
    }

    private void Awake()
    {
        if (mapScanner == null)
            mapScanner = FindFirstObjectByType<MapScanner>();
        if (grid == null)
            grid = FindFirstObjectByType<Grid>();
        if (chessPiece == null)
            chessPiece = GetComponent<ChessPiece>();

        // 렉 방지: 각 Enemy의 타이머를 랜덤으로 오프셋하여 동시 업데이트 방지
        randomOffset = Random.Range(0f, 0.5f);
        moveTimer = moveInterval + randomOffset;
    }

    private void Start()
    {
        if (grid == null)
        {
            Debug.LogError("[EnemyControl] Grid를 찾지 못했습니다.");
            enabled = false;
            return;
        }

        currentCell = grid.WorldToCell(transform.position);
        targetCell = currentCell;
        SnapToCell(currentCell);

        // MapScanner에 Enemy 위치 등록
        if (mapScanner != null)
            mapScanner.RegisterEnemy(this, currentCell);
    }

    private void OnDestroy()
    {
        // 파괴 시 MapScanner에서 위치 제거
        if (mapScanner != null)
            mapScanner.UnregisterEnemy(this);
    }

    private void Update()
    {
        // 이동 중이면 위치 업데이트
        if (isMoving)
        {
            UpdateMovement();
            return;
        }

        // 이동 간격 체크
        moveTimer -= Time.deltaTime;
        if (moveTimer <= 0f)
        {
            DecideNextMove();
            moveTimer = moveInterval;
        }
    }

    // 다음 이동을 결정하는 AI 로직
    private void DecideNextMove()
    {
        // 캐시된 이동 가능한 칸 목록 갱신 (너무 자주 계산하지 않음)
        if (Time.time - cacheUpdateTime > CACHE_UPDATE_INTERVAL)
        {
            UpdatePossibleMovesCache();
            cacheUpdateTime = Time.time;
        }

        if (cachedPossibleMoves.Count == 0)
            return;

        Vector3Int nextCell;

        switch (aiMode)
        {
            case AIMode.Random:
                // 랜덤 이동: 가능한 칸 중 하나 선택
                nextCell = cachedPossibleMoves[Random.Range(0, cachedPossibleMoves.Count)];
                break;

            case AIMode.Patrol:
                // 제자리 유지
                return;

            case AIMode.Chase:
                // 플레이어 추적
                nextCell = DecideChaseMove();
                if (nextCell == currentCell) // 이동할 곳이 없으면 대기
                    return;
                break;

            default:
                nextCell = cachedPossibleMoves[Random.Range(0, cachedPossibleMoves.Count)];
                break;
        }

        // 이동 시작
        StartMoveTo(nextCell);
    }

    // 플레이어 추적 모드의 이동 결정
    private Vector3Int DecideChaseMove()
    {
        if (mapScanner == null || !mapScanner.HeroCell.HasValue)
            return currentCell;

        Vector3Int heroPos = mapScanner.HeroCell.Value;

        // 플레이어에게 가장 가까운 칸 찾기
        Vector3Int bestCell = currentCell;
        float bestDistance = float.MaxValue;

        // 가능한 칸 중 플레이어에게 가장 가까우면서 다른 Enemy와 충돌하지 않는 칸 선택
        foreach (var cell in cachedPossibleMoves)
        {
            // 플레이어까지의 거리 계산
            float distanceToHero = Vector3Int.Distance(cell, heroPos);

            // 다른 Enemy와 너무 가까운 칸은 피하기 (꼬임 방지)
            if (IsTooCloseToOtherEnemy(cell))
                continue;

            // 더 가까운 칸이면 선택
            if (distanceToHero < bestDistance)
            {
                bestDistance = distanceToHero;
                bestCell = cell;
            }
        }

        // 모든 칸이 다른 Enemy와 너무 가까우면 그냥 가장 가까운 칸 선택
        if (bestCell == currentCell && cachedPossibleMoves.Count > 0)
        {
            bestDistance = float.MaxValue;
            foreach (var cell in cachedPossibleMoves)
            {
                float distanceToHero = Vector3Int.Distance(cell, heroPos);
                if (distanceToHero < bestDistance)
                {
                    bestDistance = distanceToHero;
                    bestCell = cell;
                }
            }
        }

        return bestCell;
    }

    // 다른 Enemy와 너무 가까운지 확인 (꼬임 방지)
    private bool IsTooCloseToOtherEnemy(Vector3Int cell)
    {
        if (mapScanner == null)
            return false;

        var enemyPositions = mapScanner.EnemyPositions;
        foreach (var kvp in enemyPositions)
        {
            Vector3Int enemyCell = kvp.Key;
            EnemyControl enemy = kvp.Value;

            // 자신은 무시
            if (enemy == this)
                continue;

            // 이미 이동 중인 Enemy의 타겟 위치도 고려
            Vector3Int enemyTarget = enemy.IsMoving ? enemy.TargetCell : enemyCell;

            // 거리가 2칸 이내면 너무 가까움
            if (Vector3Int.Distance(cell, enemyTarget) <= 2)
                return true;
        }

        return false;
    }

    // 이동 가능한 칸 목록을 캐싱하여 계산 비용 절감
    private void UpdatePossibleMovesCache()
    {
        cachedPossibleMoves.Clear();

        if (chessPiece == null || mapScanner == null)
            return;

        List<Vector3Int> moves = chessPiece.GetPossibleMoves(currentCell, mapScanner);

        // 이동 가능한 칸만 필터링 (IsWalkable 체크)
        foreach (var move in moves)
        {
            if (mapScanner.IsWalkable(move))
                cachedPossibleMoves.Add(move);
        }
    }

    // 부드러운 이동 시작
    private void StartMoveTo(Vector3Int target)
    {
        if (!mapScanner.IsWalkable(target))
            return;

        // MapScanner에 이동 시작 알림 (이전 위치 제거, 타겟 위치 등록)
        if (mapScanner != null)
            mapScanner.OnEnemyMoveStart(this, currentCell, target);

        targetCell = target;
        isMoving = true;
    }

    // 부드러운 이동 업데이트
    private void UpdateMovement()
    {
        Vector3 targetWorldPos = grid.GetCellCenterWorld(targetCell);
        Vector3 currentPos = transform.position;

        // 타겟 위치에 도달했는지 확인
        if (Vector3.Distance(currentPos, targetWorldPos) < 0.01f)
        {
            transform.position = targetWorldPos;
            currentCell = targetCell;
            isMoving = false;

            // MapScanner에 Enemy 위치 갱신
            if (mapScanner != null)
                mapScanner.RegisterEnemy(this, currentCell);

            // 충돌 체크 (Hero나 다른 Enemy와 겹쳤는지)
            CheckAndResolveCollision();

            return;
        }

        // Lerp로 부드럽게 이동
        transform.position = Vector3.MoveTowards(currentPos, targetWorldPos, moveSpeed * Time.deltaTime);
    }

    // 충돌 체크 및 해결 (Hero나 다른 Enemy와 겹쳤으면 피하기)
    private void CheckAndResolveCollision()
    {
        if (mapScanner == null)
            return;

        // Hero와 충돌했는지 확인
        if (mapScanner.HeroCell.HasValue && mapScanner.HeroCell.Value == currentCell)
        {
            Vector3Int? alternative = mapScanner.FindAlternativeCell(this, currentCell);
            if (alternative.HasValue)
            {
                // 즉시 피하기 (부드러운 이동 없이)
                currentCell = alternative.Value;
                targetCell = alternative.Value;
                SnapToCell(alternative.Value);
                mapScanner.RegisterEnemy(this, currentCell);
            }
        }
        // 다른 Enemy와 충돌했는지 확인
        else if (mapScanner.EnemyPositions.TryGetValue(currentCell, out var otherEnemy) && otherEnemy != this)
        {
            Vector3Int? alternative = mapScanner.FindAlternativeCell(this, currentCell);
            if (alternative.HasValue)
            {
                currentCell = alternative.Value;
                targetCell = alternative.Value;
                SnapToCell(alternative.Value);
                mapScanner.RegisterEnemy(this, currentCell);
            }
        }
    }

    // 즉시 특정 칸으로 이동 (스폰, 텔레포트 등에 사용)
    public void TeleportTo(Vector3Int cell)
    {
        if (!mapScanner.IsWalkable(cell))
            return;

        currentCell = cell;
        targetCell = cell;
        SnapToCell(cell);
        isMoving = false;
        UpdatePossibleMovesCache();
    }

    // 위치를 해당 칸의 중심으로 정렬
    private void SnapToCell(Vector3Int cell)
    {
        transform.position = grid.GetCellCenterWorld(cell);
    }

    // 현재 위치 반환 (다른 스크립트에서 사용 가능)
    public Vector3Int CurrentCell => currentCell;
    public Vector3Int TargetCell => targetCell;
    public bool IsMoving => isMoving;
}
