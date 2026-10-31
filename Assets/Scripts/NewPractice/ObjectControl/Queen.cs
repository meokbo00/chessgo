using System.Collections.Generic;
using UnityEngine;

// 퀸 이동 규칙. 상하좌우 + 대각선으로 Floor가 이어지는 칸까지 한 번에 갈 수 있다.
public class Queen : ChessPiece
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

    // 여덟 방향으로 레이를 쏴서 퀸이 갈 수 있는 모든 칸을 모은다.
    public override List<Vector3Int> GetPossibleMoves(Vector3Int from, MapScanner mapScanner)
    {
        var moves = new List<Vector3Int>();
        for (int i = 0; i < Directions.Length; i++)
            CollectRay(moves, from, Directions[i], mapScanner);

        return moves;
    }
}
