using System.Collections.Generic;
using UnityEngine;

public class King : ChessPiece
{
    public override List<Vector2Int> GetAvailableMoves(GameObject[,] board)
    {
        List<Vector2Int> moves = new List<Vector2Int>();
        
        // 킹이 이동하는 8가지 방향 (상하좌우 + 대각선)
        Vector2Int[] directions = {
            new Vector2Int(0, 1), new Vector2Int(0, -1), new Vector2Int(-1, 0), new Vector2Int(1, 0), // 직선
            new Vector2Int(1, 1), new Vector2Int(1, -1), new Vector2Int(-1, 1), new Vector2Int(-1, -1)  // 대각선
        };

        foreach (Vector2Int dir in directions)
        {
            int targetX = currentX + dir.x;
            int targetY = currentY + dir.y;

            // 1. 체스판 8x8 범위 안인지 확인
            if (targetX >= 0 && targetX < 8 && targetY >= 0 && targetY < 8)
            {
                GameObject targetObj = board[targetX, targetY];

                // 2. 빈 칸이면 이동 가능
                if (targetObj == null)
                {
                    moves.Add(new Vector2Int(targetX, targetY));
                }
                // 3. 누군가 있다면 적인지 확인 후 공격 가능
                else
                {
                    ChessPiece otherPiece = targetObj.GetComponent<ChessPiece>();
                    if (otherPiece != null && otherPiece.team != this.team)
                    {
                        moves.Add(new Vector2Int(targetX, targetY));
                    }
                }
            }
        }
        return moves;
    }
}