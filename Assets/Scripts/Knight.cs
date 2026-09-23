using System.Collections.Generic;
using UnityEngine;

public class Knight : ChessPiece
{
    public override List<Vector2Int> GetAvailableMoves(GameObject[,] board)
    {
        List<Vector2Int> moves = new List<Vector2Int>();
        
        // 나이트가 이동할 수 있는 8가지 방향 L자 패턴
        int[] dx = { 1, 2, 2, 1, -1, -2, -2, -1 };
        int[] dy = { 2, 1, -1, -2, -2, -1, 1, 2 };

        for (int i = 0; i < 8; i++)
        {
            int targetX = currentX + dx[i];
            int targetY = currentY + dy[i];

            // 1. 체스판 8x8 범위 안인지 확인
            if (targetX >= 0 && targetX < 8 && targetY >= 0 && targetY < 8)
            {
                GameObject targetObj = board[targetX, targetY];
                
                // 2. 빈 칸이거나
                if (targetObj == null) 
                {
                    moves.Add(new Vector2Int(targetX, targetY));
                }
                // 3. 적 기물이 있다면 이동 가능 (아군이면 불가)
                else if (targetObj.GetComponent<ChessPiece>().team != this.team) 
                {
                    moves.Add(new Vector2Int(targetX, targetY));
                }
            }
        }
        return moves;
    }
}