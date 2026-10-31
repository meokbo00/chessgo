using System.Collections.Generic;
using UnityEngine;

// 나이트 이동 규칙. L자 형태로 2칸 + 1칸 이동. 장애물을 뛰어넘을 수 있다.
public class Knight : ChessPiece
{
    private static readonly Vector3Int[] Offsets =
    {
        new Vector3Int(2, 1, 0),
        new Vector3Int(2, -1, 0),
        new Vector3Int(-2, 1, 0),
        new Vector3Int(-2, -1, 0),
        new Vector3Int(1, 2, 0),
        new Vector3Int(1, -2, 0),
        new Vector3Int(-1, 2, 0),
        new Vector3Int(-1, -2, 0)
    };

    // 여덟 개의 L자 이동 위치 중 이동 가능한 칸을 모은다.
    public override List<Vector3Int> GetPossibleMoves(Vector3Int from, MapScanner mapScanner)
    {
        var moves = new List<Vector3Int>();
        for (int i = 0; i < Offsets.Length; i++)
        {
            Vector3Int target = from + Offsets[i];
            if (mapScanner != null && mapScanner.IsWalkable(target))
                moves.Add(target);
        }

        return moves;
    }
}
