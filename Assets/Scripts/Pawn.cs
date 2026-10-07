using System.Collections.Generic;
using UnityEngine;

public class Pawn : ChessPiece
{
    public override List<Vector2Int> GetAvailableMoves(Dictionary<Vector2Int, CellType> battleMap, Dictionary<Vector2Int, ChessPiece> pieceMap)
    {
        List<Vector2Int> moves = new List<Vector2Int>();

        // 팀에 따라 전진 방향 설정: White는 위로(+1), Black은 아래로(-1)
        int direction = (team == PieceTeam.White) ? 1 : -1;

        Vector2Int forwardPos = new Vector2Int(currentX, currentY + direction);

        // 1. 기본 1칸 전진 (맵에 존재하고, 장애물이 없으며, 기물이 완벽히 비어있어야 함)
        if (battleMap.ContainsKey(forwardPos) && battleMap[forwardPos] != CellType.Blocked)
        {
            bool isForwardBlockedByPiece = pieceMap != null && pieceMap.ContainsKey(forwardPos);

            if (!isForwardBlockedByPiece)
            {
                moves.Add(forwardPos);

                // 2. 첫 턴 2칸 전진 (시작 Y 위치이고, 2칸 앞도 완전히 비어있을 때만 가능)
                bool isStartingPos = (team == PieceTeam.White && currentY == 1) || (team == PieceTeam.Black && currentY == 6);
                
                if (isStartingPos)
                {
                    Vector2Int doubleForwardPos = new Vector2Int(currentX, currentY + (direction * 2));
                    
                    if (battleMap.ContainsKey(doubleForwardPos) && battleMap[doubleForwardPos] != CellType.Blocked)
                    {
                        bool isDoubleForwardBlocked = pieceMap != null && pieceMap.ContainsKey(doubleForwardPos);
                        if (!isDoubleForwardBlocked)
                        {
                            moves.Add(doubleForwardPos);
                        }
                    }
                }
            }
        }

        // 3. 대각선 공격 (반드시 적 기물이 있는 경우만 가능)
        int[] attackX = { currentX - 1, currentX + 1 };
        
        foreach (int x in attackX)
        {
            Vector2Int attackPos = new Vector2Int(x, currentY + direction);

            // 맵에 존재하고 장애물이 아닌지 확인
            if (battleMap.ContainsKey(attackPos) && battleMap[attackPos] != CellType.Blocked)
            {
                // 해당 칸에 기물이 있고, 그 기물이 '적군'일 때만 공격 칸으로 추가
                if (pieceMap != null && pieceMap.ContainsKey(attackPos))
                {
                    if (pieceMap[attackPos].team != this.team)
                    {
                        moves.Add(attackPos);
                    }
                }
            }
        }

        return moves;
    }
}