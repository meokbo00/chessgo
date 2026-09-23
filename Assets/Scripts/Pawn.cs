using System.Collections.Generic;
using UnityEngine;

public class Pawn : ChessPiece
{
    public override List<Vector2Int> GetAvailableMoves(GameObject[,] board)
    {
        List<Vector2Int> moves = new List<Vector2Int>();

        // 팀에 따라 전진 방향 설정: White는 위로(+1), Black은 아래로(-1)
        int direction = (team == PieceTeam.White) ? 1 : -1;

        int forwardY = currentY + direction;

        // 보드 범위를 벗어나지 않는지 확인
        if (forwardY >= 0 && forwardY < 8)
        {
            // 1. 기본 1칸 전진 (반드시 빈 칸이어야 함)
            if (board[currentX, forwardY] == null)
            {
                moves.Add(new Vector2Int(currentX, forwardY));

                // 2. 첫 턴 2칸 전진 
                // 조건: 현재 위치가 시작 위치(White Y=1, Black Y=6)이고, 1칸 앞이 비어있을 때만 검사
                bool isStartingPos = (team == PieceTeam.White && currentY == 1) || (team == PieceTeam.Black && currentY == 6);
                
                if (isStartingPos)
                {
                    int doubleForwardY = currentY + (direction * 2);
                    // 2칸 앞도 비어있어야 이동 가능
                    if (board[currentX, doubleForwardY] == null)
                    {
                        moves.Add(new Vector2Int(currentX, doubleForwardY));
                    }
                }
            }

            // 3. 대각선 공격 (반드시 적 기물이 있어야 함)
            // 검사할 X 좌표: 현재 위치의 왼쪽(-1)과 오른쪽(+1)
            int[] attackX = { currentX - 1, currentX + 1 };
            
            foreach (int x in attackX)
            {
                if (x >= 0 && x < 8) // X축 보드 범위 확인
                {
                    GameObject targetObj = board[x, forwardY];
                    
                    if (targetObj != null) // 누군가 있다면
                    {
                        ChessPiece otherPiece = targetObj.GetComponent<ChessPiece>();
                        if (otherPiece != null && otherPiece.team != this.team) // 적군일 때만
                        {
                            moves.Add(new Vector2Int(x, forwardY));
                        }
                    }
                }
            }
        }

        return moves;
    }
}