using System.Collections.Generic;
using UnityEngine;

public class Queen : ChessPiece
{
    public override List<Vector2Int> GetAvailableMoves(GameObject[,] board)
    {
        List<Vector2Int> moves = new List<Vector2Int>();
        
        // 퀸은 룩과 비숍의 방향을 합친 8방향입니다.
        Vector2Int[] directions = {
            new Vector2Int(0, 1), new Vector2Int(0, -1), new Vector2Int(-1, 0), new Vector2Int(1, 0), // 직선
            new Vector2Int(1, 1), new Vector2Int(1, -1), new Vector2Int(-1, 1), new Vector2Int(-1, -1)  // 대각선
        };

        foreach (Vector2Int dir in directions)
        {
            int x = currentX;
            int y = currentY;

            while (true)
            {
                x += dir.x;
                y += dir.y;

                if (x < 0 || x >= 8 || y < 0 || y >= 8) break;

                GameObject targetObj = board[x, y];

                if (targetObj == null)
                {
                    moves.Add(new Vector2Int(x, y));
                }
                else
                {
                    ChessPiece otherPiece = targetObj.GetComponent<ChessPiece>();
                    if (otherPiece != null && otherPiece.team != this.team)
                    {
                        moves.Add(new Vector2Int(x, y));
                    }
                    break; 
                }
            }
        }
        return moves;
    }
}