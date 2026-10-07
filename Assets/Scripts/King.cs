using System.Collections.Generic;
using UnityEngine;

public class King : ChessPiece
{
    // [핵심 수정] pieceMap 매개변수를 함께 받아오도록 서명 변경
    public override List<Vector2Int> GetAvailableMoves(Dictionary<Vector2Int, CellType> battleMap, Dictionary<Vector2Int, ChessPiece> pieceMap)
    {
        List<Vector2Int> moves = new List<Vector2Int>();
        
        // 킹의 8방향 이동
        Vector2Int[] directions = {
            new Vector2Int(0, 1), new Vector2Int(0, -1), new Vector2Int(-1, 0), new Vector2Int(1, 0),
            new Vector2Int(1, 1), new Vector2Int(1, -1), new Vector2Int(-1, 1), new Vector2Int(-1, -1)
        };

        foreach (Vector2Int dir in directions)
        {
            Vector2Int targetPos = new Vector2Int(currentX + dir.x, currentY + dir.y);

            // 1. 맵 범위를 벗어난 곳(허공)이면 패스
            if (!battleMap.ContainsKey(targetPos)) continue;

            // 2. 벽이나 책상(Blocked) 장애물이면 패스
            if (battleMap[targetPos] == CellType.Blocked) continue;

            // 3. 해당 칸에 다른 기물이 존재하는 경우 검사
            if (pieceMap != null && pieceMap.ContainsKey(targetPos))
            {
                // 같은 팀 기물이 이미 자리를 차지하고 있다면 이동 불가
                if (pieceMap[targetPos].team == this.team) continue;
            }

            // 모든 검사 통과 시 이동 가능 목록에 추가
            moves.Add(targetPos);
        }

        return moves;
    }
}