using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class HeroControl : MonoBehaviour
{
    [Header("장부 및 타일맵 연동")]
    public MapScanner mapScanner;
    public Tilemap highlightTilemap;
    public Tile highlightTile;

    [Header("현재 논리 좌표")]
    public int currentX;
    public int currentY;

    [Header("상태 관리")]
    private bool isSelected = false;
    private List<Vector2Int> validMoves = new List<Vector2Int>();

    void Start()
    {
        if (mapScanner == null || mapScanner.floorTilemap == null) return;

        Vector3Int startCell = mapScanner.floorTilemap.WorldToCell(transform.position);
        currentX = startCell.x;
        currentY = startCell.y;
        transform.position = mapScanner.floorTilemap.GetCellCenterWorld(startCell);
    }

void Update()
{
    if (Keyboard.current == null || Mouse.current == null) return;
    if (mapScanner == null || mapScanner.floorTilemap == null) return;

    // 1. WASD 키보드 이동
    Vector2Int moveDir = Vector2Int.zero;
    if (Keyboard.current.wKey.wasPressedThisFrame) moveDir = new Vector2Int(0, 1);
    else if (Keyboard.current.sKey.wasPressedThisFrame) moveDir = new Vector2Int(0, -1);
    else if (Keyboard.current.aKey.wasPressedThisFrame) moveDir = new Vector2Int(-1, 0);
    else if (Keyboard.current.dKey.wasPressedThisFrame) moveDir = new Vector2Int(1, 0);

    // WASD 키 중 하나라도 눌렸다면
    if (moveDir != Vector2Int.zero)
    {
        // 하이라이트가 켜진 선택 상태였다면 즉시 하이라이트 끄기
        if (isSelected)
        {
            DeselectPiece();
        }

        // 눌린 방향으로 이동 시도
        TryMoveStep(moveDir);
        return;
    }

    // 2. 마우스 클릭 상호작용
    if (Mouse.current.leftButton.wasPressedThisFrame)
    {
        Vector3 mouseScreenPos = Mouse.current.position.ReadValue();
        mouseScreenPos.z = Mathf.Abs(Camera.main.transform.position.z - mapScanner.floorTilemap.transform.position.z);

        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(mouseScreenPos);
        Vector3Int clickedCell3D = mapScanner.floorTilemap.WorldToCell(mouseWorldPos);
        Vector2Int clickedPos = new Vector2Int(clickedCell3D.x, clickedCell3D.y);

        if (clickedPos.x == currentX && clickedPos.y == currentY)
        {
            ToggleSelection();
        }
        else if (isSelected && validMoves.Contains(clickedPos))
        {
            MoveTo(clickedPos);
        }
        else if (isSelected)
        {
            DeselectPiece();
        }
    }
}

    void TryMoveStep(Vector2Int dir)
    {
        Vector2Int targetPos = new Vector2Int(currentX + dir.x, currentY + dir.y);
        if (IsValidMove(targetPos))
        {
            MoveTo(targetPos);
        }
    }

    void ToggleSelection()
    {
        if (!isSelected)
        {
            isSelected = true;
            CalculateValidMoves();
            ShowHighlights();
            Debug.Log("기물이 선택되었습니다! 이동할 칸을 마우스로 클릭하세요.");
        }
        else
        {
            DeselectPiece();
        }
    }

    void DeselectPiece()
    {
        // 선택된 적이 없을 때는 로그나 하이라이트 정리를 건너뜁니다 (WASD 이동 시 불필요한 로그 방지)
        if (!isSelected) return;

        isSelected = false;
        validMoves.Clear();
        if (highlightTilemap != null) highlightTilemap.ClearAllTiles();
        Debug.Log("기물 선택이 해제되었습니다.");
    }

void CalculateValidMoves()
{
    validMoves.Clear();

    // 내 오브젝트에 붙어있는 ChessPiece (Bishop, Rook 등)를 가져옴
    ChessPiece piece = GetComponent<ChessPiece>();

    if (piece != null)
    {
        // 내 현재 위치 갱신
        piece.currentX = currentX;
        piece.currentY = currentY;

        // 기존 작성하신 GetAvailableMoves 함수 그대로 호출!
        validMoves = piece.GetAvailableMoves(mapScanner.battleMap);
    }
}

    bool IsValidMove(Vector2Int pos)
    {
        if (!mapScanner.battleMap.ContainsKey(pos)) return false;
        if (mapScanner.battleMap[pos] == CellType.Blocked) return false;
        return true;
    }

    void ShowHighlights()
    {
        if (highlightTilemap == null || highlightTile == null) return;
        highlightTilemap.ClearAllTiles();

        foreach (var pos in validMoves)
        {
            Vector3Int cellPos3D = new Vector3Int(pos.x, pos.y, 0);
            highlightTilemap.SetTile(cellPos3D, highlightTile);
        }
    }

    void MoveTo(Vector2Int targetPos)
    {
        currentX = targetPos.x;
        currentY = targetPos.y;

        Vector3Int targetCellPos = new Vector3Int(currentX, currentY, 0);
        transform.position = mapScanner.floorTilemap.GetCellCenterWorld(targetCellPos);

        DeselectPiece();
    }
}