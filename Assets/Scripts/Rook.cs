using System.Collections.Generic;
using UnityEngine;

public class Rook : ChessPiece
{
    public override List<Vector2Int> GetAvailableMoves(Dictionary<Vector2Int, CellType> battleMap, Dictionary<Vector2Int, ChessPiece> pieceMap)
    {
        List<Vector2Int> moves = new List<Vector2Int>();
        
        // 룩이 이동하는 4가지 방향 (상, 하, 좌, 우)
        Vector2Int[] directions = {
            new Vector2Int(0, 1),  // 위
            new Vector2Int(0, -1), // 아래
            new Vector2Int(-1, 0), // 왼쪽
            new Vector2Int(1, 0)   // 오른쪽
        };

        foreach (Vector2Int dir in directions)
        {
            int step = 1;
            while (true)
            {
                Vector2Int targetPos = new Vector2Int(currentX + (dir.x * step), currentY + (dir.y * step));

                // 1. 맵 바깥(허공)이면 이 방향 탐색 중단
                if (!battleMap.ContainsKey(targetPos)) break;

                // 2. 벽이나 책상(Blocked) 등 장애물이 막고 있다면 탐색 중단
                if (battleMap[targetPos] == CellType.Blocked) break;

                // 3. 기물이 배치되어 있는지 검사
                if (pieceMap != null && pieceMap.ContainsKey(targetPos))
                {
                    // 적 기물인 경우 공격할 수 있으므로 해당 칸까지 추가 후 전진 중단
                    if (pieceMap[targetPos].team != this.team)
                    {
                        moves.Add(targetPos);
                    }
                    // 아군이든 적군이든 기물에 가로막혔으므로 전진 중단
                    break;
                }

                // 4. 빈 바닥인 경우 이동 가능 목록에 넣고 계속 직진
                moves.Add(targetPos);
                step++;
            }
        }

        return moves;
    }
}