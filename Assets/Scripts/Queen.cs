using System.Collections.Generic;
using UnityEngine;

public class Queen : ChessPiece
{
    public override List<Vector2Int> GetAvailableMoves(Dictionary<Vector2Int, CellType> battleMap, Dictionary<Vector2Int, ChessPiece> pieceMap)
    {
        List<Vector2Int> moves = new List<Vector2Int>();
        
        // 퀸은 룩(직선 4방향)과 비숍(대각선 4방향)을 합친 8방향
        Vector2Int[] directions = {
            new Vector2Int(0, 1), new Vector2Int(0, -1), new Vector2Int(-1, 0), new Vector2Int(1, 0), // 직선
            new Vector2Int(1, 1), new Vector2Int(1, -1), new Vector2Int(-1, 1), new Vector2Int(-1, -1)  // 대각선
        };

        foreach (Vector2Int dir in directions)
        {
            int step = 1;
            while (true)
            {
                Vector2Int targetPos = new Vector2Int(currentX + (dir.x * step), currentY + (dir.y * step));

                // 1. 맵 바깥(허공)이면 이 방향 탐색 중단
                if (!battleMap.ContainsKey(targetPos)) break;

                // 2. 벽이나 책상(Blocked) 장애물이 막고 있다면 탐색 중단
                if (battleMap[targetPos] == CellType.Blocked) break;

                // 3. 기물이 배치되어 있는지 검사
                if (pieceMap != null && pieceMap.ContainsKey(targetPos))
                {
                    // 적 기물인 경우 잡아먹을 수 있으므로 해당 칸까지 추가 후 전진 중단
                    if (pieceMap[targetPos].team != this.team)
                    {
                        moves.Add(targetPos);
                    }
                    // 기물에 가로막혔으므로 더 이상 전진 중단
                    break;
                }

                // 4. 빈 칸인 경우 이동 가능 목록에 넣고 계속 전진
                moves.Add(targetPos);
                step++;
            }
        }

        return moves;
    }
}