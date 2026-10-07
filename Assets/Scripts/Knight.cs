using System.Collections.Generic;
using UnityEngine;

public class Knight : ChessPiece
{
    // [핵심 수정] pieceMap 매개변수를 함께 받아오도록 서명 변경
    public override List<Vector2Int> GetAvailableMoves(Dictionary<Vector2Int, CellType> battleMap, Dictionary<Vector2Int, ChessPiece> pieceMap)
    {
        List<Vector2Int> moves = new List<Vector2Int>();
        
        // 나이트의 L자 8방향
        int[] dx = { 1, 2, 2, 1, -1, -2, -2, -1 };
        int[] dy = { 2, 1, -1, -2, -2, -1, 1, 2 };

        for (int i = 0; i < 8; i++)
        {
            Vector2Int targetPos = new Vector2Int(currentX + dx[i], currentY + dy[i]);

            // 1. 맵 범위를 벗어난 곳이면 패스
            if (!battleMap.ContainsKey(targetPos)) continue;

            // 2. 착지할 칸이 벽이나 장애물이면 패스
            if (battleMap[targetPos] == CellType.Blocked) continue;

            // 3. 해당 칸에 다른 기물이 존재하는 경우 검사
            if (pieceMap != null && pieceMap.ContainsKey(targetPos))
            {
                // 같은 팀 기물이 이미 자리를 차지하고 있다면 이동 불가
                if (pieceMap[targetPos].team == this.team) continue;
            }

            moves.Add(targetPos);
        }

        return moves;
    }
}