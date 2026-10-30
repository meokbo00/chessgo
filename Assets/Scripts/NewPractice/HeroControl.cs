using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

// WASD 한 칸 이동과, 클릭으로 기물 규칙에 맞는 칸 표시/이동을 담당한다.
public class HeroControl : MonoBehaviour
{
    [SerializeField] private MapScanner mapScanner;
    [SerializeField] private Grid grid;
    [Header("이동 가능 칸 나타낼 타일 이미지")]
    [SerializeField] private Tile highlightTile;
    [Header("타일 이미지를 표시할 타일맵")]
    [SerializeField] private Tilemap highlightTilemap;
    [Header("상속 받을 체스 움직임 룰")]
    [SerializeField] private ChessPiece chessPiece;

    private Vector3Int currentCell;
    private readonly HashSet<Vector3Int> possibleMoves = new();
    private bool isShowingMoves;

    // 더블탭 상호작용 관련 변수
    [Header("상호작용 설정")]
    [SerializeField] private float doubleTapWindow = 0.3f;
    private Vector3Int pendingInteractCell;
    private Vector3Int pendingInteractDirection;
    private float pendingInteractTime = -1f;

    // 참조가 비어 있으면 씬/같은 오브젝트에서 필요한 컴포넌트를 찾아 채운다.
    private void Awake()
    {
        if (mapScanner == null)
            mapScanner = FindFirstObjectByType<MapScanner>();
        if (grid == null)
            grid = FindFirstObjectByType<Grid>();
        if (chessPiece == null)
            chessPiece = GetComponent<ChessPiece>();
        if (highlightTilemap == null)
            highlightTilemap = FindFirstObjectByType<Tilemap>();
        if (highlightTile == null)
            highlightTile = FindFirstObjectByType<Tile>();
    }

    // 시작 위치를 가장 가까운 격자 칸 중심으로 맞춘다.
    private void Start()
    {
        if (grid == null)
        {
            Debug.LogError("[HeroControl] Grid를 찾지 못했습니다.");
            enabled = false;
            return;
        }

        currentCell = grid.WorldToCell(transform.position);
        SnapToCurrentCell();

        // MapScanner에 Hero 위치 등록
        if (mapScanner != null)
            mapScanner.RegisterHero(currentCell);
    }

    // WASD 한 칸 이동과 마우스 클릭(경로 표시/이동)을 처리한다.
    private void Update()
    {
        HandleMouseClick();

        Vector3Int direction = ReadMoveDirection();
        if (direction == Vector3Int.zero)
            return;

        TryMove(direction);
    }

    // 클릭한 칸이 기물이면 가능 경로를 켜고, 하이라이트 칸이면 그쪽으로 이동한다.
    // InteractableObject를 클릭하고 플레이어가 인접해 있으면 상호작용 실행.
    private void HandleMouseClick()
    {
        if (!Input.GetMouseButtonDown(0))
            return;
        if (!TryGetClickedCell(out Vector3Int clickedCell))
            return;

        // InteractableObject 상호작용 체크
        if (TryInteractWithObject(clickedCell))
            return;

        if (isShowingMoves && possibleMoves.Contains(clickedCell))
        {
            MoveTo(clickedCell);
            HideHighlightTile();
            return;
        }

        if (clickedCell == currentCell)
        {
            if (isShowingMoves)
                HideHighlightTile();
            else
                ShowPossibleMoves();
            return;
        }

        HideHighlightTile();
    }

    // 클릭한 위치에 InteractableObject가 있고 플레이어가 인접해 있으면 상호작용 실행
    private bool TryInteractWithObject(Vector3Int clickedCell)
    {
        if (mapScanner == null)
            return false;

        if (!mapScanner.Interactables.TryGetValue(clickedCell, out var interactableList))
            return false;

        // isInteractable이 true인 오브젝트 찾기
        InteractableObject targetInteractable = null;
        foreach (var obj in interactableList)
        {
            if (obj.IsInteractable)
            {
                targetInteractable = obj;
                break;
            }
        }

        if (targetInteractable == null)
            return false;

        // 플레이어가 오브젝트에 인접해 있는지 확인
        if (!IsPlayerAdjacentToObject(targetInteractable))
            return false;

        // 상호작용 실행
        targetInteractable.Interact();
        return true;
    }

    // 플레이어가 InteractableObject에 인접해 있는지 확인 (8방향 모두 포함)
    private bool IsPlayerAdjacentToObject(InteractableObject obj)
    {
        if (mapScanner == null || grid == null)
            return false;

        // 오브젝트가 점유하는 모든 칸 계산
        Vector3Int anchorCell = grid.WorldToCell(obj.transform.position);
        HashSet<Vector3Int> occupiedCells = new HashSet<Vector3Int>();

        for (int x = 0; x < obj.WidthInCells; x++)
        {
            for (int y = 0; y < obj.HeightInCells; y++)
            {
                Vector3Int cell = anchorCell + new Vector3Int(x, y, 0);
                occupiedCells.Add(cell);
            }
        }

        // 8방향 오프셋
        Vector3Int[] directions = {
            Vector3Int.up, Vector3Int.down, Vector3Int.left, Vector3Int.right,
            new Vector3Int(1, 1, 0), new Vector3Int(1, -1, 0),
            new Vector3Int(-1, 1, 0), new Vector3Int(-1, -1, 0)
        };

        // 오브젝트가 점유하는 모든 칸의 8방향에 플레이어가 있는지 확인
        foreach (var cell in occupiedCells)
        {
            foreach (var dir in directions)
            {
                Vector3Int adjacentCell = cell + dir;
                if (adjacentCell == currentCell)
                    return true;
            }
        }

        return false;
    }

    // 화면 클릭 위치를 격자 칸 좌표로 바꾼다.
    private bool TryGetClickedCell(out Vector3Int cell)
    {
        cell = Vector3Int.zero;
        Camera cam = Camera.main;
        if (cam == null)
            return false;

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        var plane = new Plane(Vector3.forward, Vector3.zero);
        if (!plane.Raycast(ray, out float distance))
            return false;

        cell = grid.WorldToCell(ray.GetPoint(distance));
        return true;
    }

    // 붙어 있는 ChessPiece(지금은 Queen)에게 가능 칸을 받아 하이라이트 타일로 그린다.
    private void ShowPossibleMoves()
    {
        if (chessPiece == null || mapScanner == null || highlightTile == null)
            return;

        HideHighlightTile();

        List<Vector3Int> moves = chessPiece.GetPossibleMoves(currentCell, mapScanner);
        for (int i = 0; i < moves.Count; i++)
        {
            Vector3Int cell = moves[i];
            possibleMoves.Add(cell);
            highlightTilemap.SetTile(cell, highlightTile);
        }

        isShowingMoves = true;
    }

    // 표시 중이던 이동 가능 칸을 지운다.
    private void HideHighlightTile()
    {
        if (highlightTilemap != null)
            highlightTilemap.ClearAllTiles();

        possibleMoves.Clear();
        isShowingMoves = false;
    }

    // W/A/S/D 키다운을 격자 방향 벡터로 바꾼다. 동시에 눌리면 하나만 적용.
    private static Vector3Int ReadMoveDirection()
    {
        if (Input.GetKeyDown(KeyCode.W))
            return Vector3Int.up;
        if (Input.GetKeyDown(KeyCode.S))
            return Vector3Int.down;
        if (Input.GetKeyDown(KeyCode.A))
            return Vector3Int.left;
        if (Input.GetKeyDown(KeyCode.D))
            return Vector3Int.right;

        return Vector3Int.zero;
    }

    // 목표 칸이 Floor일 때만 한 칸 이동한다. 상호작용 오브젝트가 있으면 더블탭 체크.
    private void TryMove(Vector3Int direction)
    {
        Vector3Int nextCell = currentCell + direction;

        // 상호작용 오브젝트가 있는지 확인
        if (mapScanner != null && mapScanner.Interactables.TryGetValue(nextCell, out var interactableList))
        {
            // isInteractable이 true인 오브젝트를 우선 찾음
            InteractableObject targetInteractable = null;
            foreach (var obj in interactableList)
            {
                if (obj.IsInteractable)
                {
                    targetInteractable = obj;
                    break;
                }
            }

            // 상호작용 가능한 오브젝트가 있으면 더블탭 체크
            if (targetInteractable != null)
            {
                // 더블탭인지 확인 (같은 칸, 같은 방향, 시간 윈도우 내)
                bool isDoubleTap =
                    pendingInteractCell == nextCell &&
                    pendingInteractDirection == direction &&
                    Time.time - pendingInteractTime <= doubleTapWindow;

                if (isDoubleTap)
                {
                    // 더블탭이면 상호작용 실행
                    targetInteractable.Interact();
                    ResetPendingInteraction();
                }
                else
                {
                    // 첫 탭이면 대기 상태로 진입
                    pendingInteractCell = nextCell;
                    pendingInteractDirection = direction;
                    pendingInteractTime = Time.time;
                }

                // 상호작용 오브젝트가 있는 칸은 이동하지 않음
                return;
            }
        }

        // 다른 칸으로 이동하면 대기 상태 취소
        ResetPendingInteraction();

        if (!CanEnter(nextCell))
            return;

        MoveTo(nextCell);
        HideHighlightTile();
    }

    // 상호작용 대기 상태를 초기화한다.
    private void ResetPendingInteraction()
    {
        pendingInteractTime = -1f;
        pendingInteractCell = new Vector3Int(int.MinValue, int.MinValue, int.MinValue);
        pendingInteractDirection = Vector3Int.zero;
    }

    // 검증된 칸으로 기물을 즉시 옮긴다. 힘은 쓰지 않고 좌표만 바꾼다.
    private void MoveTo(Vector3Int cell)
    {
        currentCell = cell;
        SnapToCurrentCell();

        // MapScanner에 Hero 위치 갱신
        if (mapScanner != null)
            mapScanner.RegisterHero(currentCell);
    }

    // Blocked, Cliff(맵 밖 포함)는 들어갈 수 없다.
    private bool CanEnter(Vector3Int cell)
    {
        if (mapScanner == null)
            return false;

        return mapScanner.IsWalkable(cell);
    }

    // 기물 위치를 현재 격자 칸의 한가운데로 맞춘다.
    private void SnapToCurrentCell()
    {
        transform.position = grid.GetCellCenterWorld(currentCell);
    }
}
