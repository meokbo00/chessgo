using System.Collections.Generic;
using UnityEngine;

public enum PieceTeam { White, Black } // 팀 구분

public abstract class ChessPiece : MonoBehaviour
{
    public PieceTeam team;
    public int currentX;
    public int currentY;

    // 타일맵 지형 정보(mapData)와 필드의 기물 정보(units)를 전달받아 이동 가능 칸 반환
    public abstract List<Vector2Int> GetAvailableMoves(
        Dictionary<Vector2Int, CellType> mapData, 
        Dictionary<Vector2Int, ChessPiece> units = null
    );
}