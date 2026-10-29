using System.Collections.Generic;
using UnityEngine;

// 킹 이동 규칙. 상하좌우 + 대각선으로 1칸씩만 이동할 수 있다.
public class King : ChessPiece
{
    private static readonly Vector3Int[] Directions =
    {
        Vector3Int.up,
        Vector3Int.down,
        Vector3Int.left,
        Vector3Int.right,
        new Vector3Int(1, 1, 0),
        new Vector3Int(1, -1, 0),
        new Vector3Int(-1, 1, 0),
        new Vector3Int(-1, -1, 0)
    };

    // 여덟 방향으로 1칸씩만 킹이 갈 수 있는 칸을 모은다.
    public override List<Vector3Int> GetPossibleMoves(Vector3Int from, MapScanner mapScanner)
    {
        var moves = new List<Vector3Int>();
        for (int i = 0; i < Directions.Length; i++)
        {
            Vector3Int target = from + Directions[i];
            if (mapScanner != null && mapScanner.IsWalkable(target))
                moves.Add(target);
        }

        return moves;
    }
}
