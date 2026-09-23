using System.Collections.Generic;
using UnityEngine;

public enum PieceTeam { White, Black } // 팀 구분

public abstract class ChessPiece : MonoBehaviour
{
    public PieceTeam team;
    public int currentX;
    public int currentY;

    // 핵심: 자식 기물(나이트, 룩 등)이 무조건 자기만의 규칙으로 덮어써야(Override) 하는 함수
    // 현재 보드 상태를 보고, 자신이 이동할 수 있는 모든 좌표 리스트를 반환합니다.
    public abstract List<Vector2Int> GetAvailableMoves(GameObject[,] board);
}