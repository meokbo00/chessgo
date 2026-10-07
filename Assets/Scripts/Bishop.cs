using System.Collections.Generic;
using UnityEngine;

public class Bishop : ChessPiece
{
    public override List<Vector2Int> GetAvailableMoves(
        Dictionary<Vector2Int, CellType> mapData, 
        Dictionary<Vector2Int, ChessPiece> units = null
    )
    {
        List<Vector2Int> moves = new List<Vector2Int>();

        // 비숍이 이동하는 4가지 방향 (우상, 우하, 좌상, 좌하)
        Vector2Int[] directions = {
            new Vector2Int(1, 1),
            new Vector2Int(1, -1),
            new Vector2Int(-1, 1),
            new Vector2Int(-1, -1)
        };

        foreach (Vector2Int dir in directions)
        {
            int x = currentX;
            int y = currentY;

            while (true)
            {
                x += dir.x;
                y += dir.y;
                Vector2Int checkPos = new Vector2Int(x, y);

                // 1. [방 범위 검사] 타일맵에 없는 칸(허공)이면 전진 중단
                if (!mapData.ContainsKey(checkPos)) break;

                // 2. [지형 장애물 검사] 벽이나 책상(Blocked)이면 전진 중단
                if (mapData[checkPos] == CellType.Blocked) break;

                // 3. [기물 검사] 해당 위치에 다른 기물이 존재하는 경우
                if (units != null && units.ContainsKey(checkPos))
                {
                    ChessPiece otherPiece = units[checkPos];
                    // 적군 기물이라면 공격 가능 칸으로 추가 후 전진 중단
                    if (otherPiece != null && otherPiece.team != this.team)
                    {
                        moves.Add(checkPos);
                    }
                    // 아군이든 적군이든 기물을 만났으므로 기물 너머로는 통과 불가
                    break;
                }

                // 4. 아무것도 없는 빈 바닥이면 이동 가능 칸 추가 후 계속 전진!
                moves.Add(checkPos);
            }
        }
        return moves;
    }
}