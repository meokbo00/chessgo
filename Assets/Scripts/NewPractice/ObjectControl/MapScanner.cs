using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

// 씬의 타일맵을 읽어 격자 칸마다 CellType을 정하고, 시작 시 개수를 로그로 남긴다.
public class MapScanner : MonoBehaviour
{
    [Header("Tilemaps")]
    [SerializeField] private Tilemap floorTile;
    [SerializeField] private Tilemap interriorTile;
    [SerializeField] private Tilemap wallTile;
    [SerializeField] private Tilemap obstacleTile;

    // 타일맵 → CellType 임시 매핑. 인스펙터에서 바꿔도 됨.
    [Header("CellType Mapping")]
    [SerializeField] private CellType floorTileType = CellType.Floor;
    [SerializeField] private CellType interriorTileType = CellType.Blocked;
    [SerializeField] private CellType wallTileType = CellType.Blocked;
    [SerializeField] private CellType obstacleTileType = CellType.Blocked;

    // 격자 좌표별 최종 셀 타입. 겹치는 타일은 우선순위가 높은 쪽이 남음.
    private readonly Dictionary<Vector3Int, CellType> cells = new();

    // 상호작용 가능한 오브젝트들을 좌표별로 관리 (한 칸에 여러 오브젝트 가능)
    private readonly Dictionary<Vector3Int, List<InteractableObject>> interactables = new();

    // Hero와 Enemy의 위치 추적 (충돌 방지용)
    private Vector3Int? heroCell;
    private readonly Dictionary<Vector3Int, EnemyControl> enemyPositions = new();

    public IReadOnlyDictionary<Vector3Int, CellType> Cells => cells;
    public IReadOnlyDictionary<Vector3Int, List<InteractableObject>> Interactables => interactables;
    public Vector3Int? HeroCell => heroCell;
    public IReadOnlyDictionary<Vector3Int, EnemyControl> EnemyPositions => enemyPositions;

    // 다른 스크립트(Start)보다 먼저 맵을 읽어 두기 위해 Awake에서 스캔한다.
    private void Awake()
    {
        ResolveTilemaps();
        Scan();
        ScanInteractables();
    }

    // 스캔이 끝난 뒤 타일/셀 개수를 콘솔에 출력한다.
    private void Start()
    {
        LogScanResult();
    }

    // 해당 격자 칸의 CellType을 반환한다. 맵에 없는 칸은 절벽(낙사)으로 취급.
    public CellType GetCellType(Vector3Int cell)
    {
        return cells.TryGetValue(cell, out var type) ? type : CellType.Cliff;
    }

    // Floor만 이동 가능. Blocked와 Cliff는 false. InteractableObject 중 isBlock이 있는 게 있으면 false.
    // Hero나 Enemy가 있는 칸도 이동 불가.
    public bool IsWalkable(Vector3Int cell)
    {
        if (GetCellType(cell) != CellType.Floor)
            return false;

        if (interactables.TryGetValue(cell, out var objList))
        {
            foreach (var obj in objList)
            {
                if (obj.IsBlock)
                    return false;
            }
        }

        // Hero가 있는 칸은 이동 불가
        if (heroCell.HasValue && heroCell.Value == cell)
            return false;

        // Enemy가 있는 칸은 이동 불가
        if (enemyPositions.ContainsKey(cell))
            return false;

        return true;
    }

    // Hero의 위치를 등록/갱신한다.
    public void RegisterHero(Vector3Int cell)
    {
        heroCell = cell;
    }

    // Hero의 위치를 제거한다.
    public void UnregisterHero()
    {
        heroCell = null;
    }

    // Enemy의 위치를 등록/갱신한다.
    public void RegisterEnemy(EnemyControl enemy, Vector3Int cell)
    {
        if (enemy == null)
            return;

        // 이동 중이면 점유가 targetCell에 있어서 CurrentCell만 지우면
        // 빈 칸에 유령 점유가 남는다. 이 기물이 점유한 칸을 모두 지운다.
        RemoveAllEntriesOf(enemy);
        enemyPositions[cell] = enemy;
    }

    // Enemy의 이동을 시작할 때 호출 (이전 위치 제거, 타겟 위치 등록)
    public void OnEnemyMoveStart(EnemyControl enemy, Vector3Int fromCell, Vector3Int toCell)
    {
        RegisterEnemy(enemy, toCell);
    }

    // Enemy의 위치를 제거한다.
    public void UnregisterEnemy(EnemyControl enemy)
    {
        RemoveAllEntriesOf(enemy);
    }

    private void RemoveAllEntriesOf(EnemyControl enemy)
    {
        if (enemy == null)
            return;

        List<Vector3Int> keysToRemove = null;

        foreach (var kvp in enemyPositions)
        {
            if (kvp.Value != enemy)
                continue;

            keysToRemove ??= new List<Vector3Int>();
            keysToRemove.Add(kvp.Key);
        }

        if (keysToRemove == null)
            return;

        foreach (Vector3Int key in keysToRemove)
            enemyPositions.Remove(key);
    }

    // 충돌 시 Enemy가 피할 수 있는 주변 칸을 찾는다.
    public Vector3Int? FindAlternativeCell(EnemyControl enemy, Vector3Int originalTarget)
    {
        if (enemy == null)
            return null;

        // 8방향 탐색
        Vector3Int[] directions = {
            Vector3Int.up, Vector3Int.down, Vector3Int.left, Vector3Int.right,
            new Vector3Int(1, 1, 0), new Vector3Int(1, -1, 0),
            new Vector3Int(-1, 1, 0), new Vector3Int(-1, -1, 0)
        };

        // 원래 타겟 주변 칸 탐색
        foreach (var dir in directions)
        {
            Vector3Int candidate = originalTarget + dir;

            // 이동 가능한지 확인
            if (!IsWalkable(candidate))
                continue;

            // 다른 Enemy가 있는지 확인 (자신 제외)
            if (enemyPositions.TryGetValue(candidate, out var otherEnemy) && otherEnemy != enemy)
                continue;

            // Hero가 있는지 확인
            if (heroCell.HasValue && heroCell.Value == candidate)
                continue;

            return candidate;
        }

        // 원래 타겟 주변에 없으면 현재 위치 주변 탐색
        Vector3Int currentPos = enemy.CurrentCell;
        foreach (var dir in directions)
        {
            Vector3Int candidate = currentPos + dir;

            if (!IsWalkable(candidate))
                continue;

            if (enemyPositions.TryGetValue(candidate, out var otherEnemy) && otherEnemy != enemy)
                continue;

            if (heroCell.HasValue && heroCell.Value == candidate)
                continue;

            return candidate;
        }

        // 피할 곳이 없으면 null 반환
        return null;
    }

    // 해당 칸이 스캔 결과에 있으면 true와 함께 타입을 넘겨준다. 없으면 false.
    public bool TryGetCellType(Vector3Int cell, out CellType type)
    {
        return cells.TryGetValue(cell, out type);
    }

    // 인스펙터에 안 넣어도 씬 오브젝트 이름으로 타일맵을 찾는다.
    private void ResolveTilemaps()
    {
        if (floorTile == null)
            floorTile = FindTilemapByName("FloorTile");
        if (interriorTile == null)
            interriorTile = FindTilemapByName("InterriorTile") ?? FindTilemapByName("InteriorTile");
        if (wallTile == null)
            wallTile = FindTilemapByName("WallTile");
        if (obstacleTile == null)
            obstacleTile = FindTilemapByName("ObstacleTile");
    }

    // 이름으로 GameObject를 찾아 Tilemap 컴포넌트를 가져온다.
    private static Tilemap FindTilemapByName(string objectName)
    {
        var go = GameObject.Find(objectName);
        return go != null ? go.GetComponent<Tilemap>() : null;
    }

    // 모든 타일맵 레이어를 읽어 cells 딕셔너리를 채운다. 바닥을 먼저 넣고 장애물/벽이 덮어쓴다.
    private void Scan()
    {
        cells.Clear();

        ApplyLayer(floorTile, floorTileType);
        ApplyLayer(interriorTile, interriorTileType);
        ApplyLayer(wallTile, wallTileType);
        ApplyLayer(obstacleTile, obstacleTileType);
    }

    // 씬에 있는 모든 InteractableObject를 찾아 점유하는 타일을 등록한다.
    private void ScanInteractables()
    {
        interactables.Clear();

        InteractableObject[] objects = FindObjectsByType<InteractableObject>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var obj in objects)
        {
            RegisterInteractable(obj);
        }
    }

    // InteractableObject가 점유하는 모든 타일을 딕셔너리에 등록한다.
    private void RegisterInteractable(InteractableObject obj)
    {
        if (obj == null || floorTile == null)
            return;

        Vector3Int anchorCell = floorTile.WorldToCell(obj.transform.position);

        for (int x = 0; x < obj.WidthInCells; x++)
        {
            for (int y = 0; y < obj.HeightInCells; y++)
            {
                Vector3Int cell = anchorCell + new Vector3Int(x, y, 0);

                if (!interactables.ContainsKey(cell))
                    interactables[cell] = new List<InteractableObject>();

                interactables[cell].Add(obj);
            }
        }
    }

    // 한 타일맵에 깔린 타일마다 지정한 CellType을 기록한다. 우선순위가 더 높은 타입만 기존 값을 덮는다.
    private void ApplyLayer(Tilemap tilemap, CellType type)
    {
        if (tilemap == null)
            return;

        foreach (var pos in tilemap.cellBounds.allPositionsWithin)
        {
            if (!tilemap.HasTile(pos))
                continue;

            // 우선순위가 같거나 낮으면 기존 값을 유지 (Floor가 벽을 지우지 않음).
            if (!cells.TryGetValue(pos, out var existing) || GetPriority(type) > GetPriority(existing))
                cells[pos] = type;
        }
    }

    // CellType끼리 겹칠 때 누가 이길지 정한다. 숫자가 클수록 강함 (Cliff > Blocked > Floor).
    private static int GetPriority(CellType type)
    {
        switch (type)
        {
            case CellType.Floor:
                return 0;
            case CellType.Blocked:
                return 1;
            case CellType.Cliff:
                return 2;
            default:
                return 0;
        }
    }

    // 깔린 타일 수와 CellType별 칸 수를 콘솔에 출력한다.
    private void LogScanResult()
    {
        int floorLayerCount = CountTiles(floorTile);
        int interiorLayerCount = CountTiles(interriorTile);
        int wallLayerCount = CountTiles(wallTile);
        int obstacleLayerCount = CountTiles(obstacleTile);
        int placedTileCount = floorLayerCount + interiorLayerCount + wallLayerCount + obstacleLayerCount;

        int floorCells = 0;
        int blockedCells = 0;
        int cliffCells = 0;

        foreach (var type in cells.Values)
        {
            switch (type)
            {
                case CellType.Floor:
                    floorCells++;
                    break;
                case CellType.Blocked:
                    blockedCells++;
                    break;
                case CellType.Cliff:
                    cliffCells++;
                    break;
            }
        }

        Debug.Log(
            $"[MapScanner] 깔린 타일 {placedTileCount}개\n" +
            $"  타일맵: FloorTile {floorLayerCount}, InterriorTile {interiorLayerCount}, WallTile {wallLayerCount}, ObstacleTile {obstacleLayerCount}\n" +
            $"  격자 셀 {cells.Count}개 → Floor {floorCells}, Blocked {blockedCells}, Cliff {cliffCells}\n" +
            $"  매핑: FloorTile={floorTileType}, InterriorTile={interriorTileType}, WallTile={wallTileType}, ObstacleTile={obstacleTileType}");
    }

    // 해당 타일맵에 실제로 배치된 타일 개수를 센다.
    private static int CountTiles(Tilemap tilemap)
    {
        if (tilemap == null)
            return 0;

        int count = 0;
        foreach (var pos in tilemap.cellBounds.allPositionsWithin)
        {
            if (tilemap.HasTile(pos))
                count++;
        }

        return count;
    }
}
