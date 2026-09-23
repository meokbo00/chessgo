using System.Collections.Generic;
using UnityEngine;

public class Rook : ChessPiece
{
    public override List<Vector2Int> GetAvailableMoves(GameObject[,] board)
    {
        List<Vector2Int> moves = new List<Vector2Int>();
        
        // 룩이 이동하는 4가지 방향 (상, 하, 좌, 우)
        Vector2Int[] directions = {
            new Vector2Int(0, 1),  // 위
            new Vector2Int(0, -1), // 아래
            new Vector2Int(-1, 0), // 왼쪽
            new Vector2Int(1, 0)   // 오른쪽
        };

        // 4방향을 각각 한 줄씩 끝까지 검사합니다.
        foreach (Vector2Int dir in directions)
        {
            int x = currentX;
            int y = currentY;

            while (true) // 계속 직진!
            {
                x += dir.x;
                y += dir.y;

                // 1. 체스판 범위를 벗어나면 이 방향은 탐색을 멈춥니다.
                if (x < 0 || x >= 8 || y < 0 || y >= 8)
                    break;

                GameObject targetObj = board[x, y];

                // 2. 빈 칸인 경우: 이동 가능 목록에 넣고 '다음 칸'으로 계속 전진
                if (targetObj == null)
                {
                    moves.Add(new Vector2Int(x, y));
                }
                // 3. 기물에 막힌 경우
                else
                {
                    ChessPiece otherPiece = targetObj.GetComponent<ChessPiece>();
                    
                    // 만약 적군이라면 잡아먹을 수 있으므로 그 칸(적의 위치)까지는 갈 수 있습니다.
                    if (otherPiece != null && otherPiece.team != this.team)
                    {
                        moves.Add(new Vector2Int(x, y));
                    }
                    
                    // 적군이든 아군이든 누군가에게 막혔다면 그 너머로는 못 가므로 전진을 멈춥니다.
                    break; 
                }
            }
        }

        return moves;
    }
}