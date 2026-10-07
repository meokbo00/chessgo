using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MapScanner : MonoBehaviour
{
    [Header("타일맵 연결")]
    public Tilemap floorTilemap;     // 바닥 타일맵
    public Tilemap obstacleTilemap;  // 장애물 타일맵

    // 이번 방의 모든 좌표와 지형 정보를 담는 장부 (딕셔너리)
    public Dictionary<Vector2Int, CellType> battleMap = new Dictionary<Vector2Int, CellType>();

    void Start()
    {
        ScanMap();
    }

    void ScanMap()
    {
        battleMap.Clear();

        // 1. 타일맵에 그림이 그려져 있는 전체 영역의 범위(Bounds)를 가져옵니다.
        BoundsInt bounds = floorTilemap.cellBounds;

        // 2. 범위 안의 모든 칸을 하나씩 검사합니다.
        foreach (var pos in bounds.allPositionsWithin)
        {
            Vector3Int cellPos3D = new Vector3Int(pos.x, pos.y, 0);
            Vector2Int cellPos2D = new Vector2Int(pos.x, pos.y);

            // 바닥(Floor) 그림이 아예 없는 곳은 맵의 존재하지 않는 공간(절벽/허공)이므로 패스!
            if (!floorTilemap.HasTile(cellPos3D)) continue;

            // 3. 그 자리에 장애물(Obstacle) 타일이 덧칠되어 있다면 Blocked, 아니면 Ground로 등록
            if (obstacleTilemap != null && obstacleTilemap.HasTile(cellPos3D))
            {
                battleMap[cellPos2D] = CellType.Blocked;
            }
            else
            {
                battleMap[cellPos2D] = CellType.Ground;
            }
        }

        Debug.Log($"[MapScanner] 맵 스캔 완료! 총 {battleMap.Count}개의 칸이 정상적으로 등록되었습니다.");
    }
}