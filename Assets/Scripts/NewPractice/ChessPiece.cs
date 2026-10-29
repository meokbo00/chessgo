using System.Collections.Generic;
using UnityEngine;

// 기물별 이동 규칙을 담는 부모. HeroControl은 이 컴포넌트에게 가능한 칸만 물어본다.
public abstract class ChessPiece : MonoBehaviour
{
    // 지금 칸에서 한 번에 갈 수 있는 격자 좌표 목록을 반환한다.
    public abstract List<Vector3Int> GetPossibleMoves(Vector3Int from, MapScanner mapScanner);

    // 한 방향으로 Floor가 이어지는 동안 칸을 모은다. 벽/절벽을 만나면 그 줄은 끝.
    protected static void CollectRay(List<Vector3Int> moves, Vector3Int from, Vector3Int direction, MapScanner mapScanner)
    {
        if (mapScanner == null)
            return;

        Vector3Int cell = from + direction;
        while (mapScanner.IsWalkable(cell))
        {
            moves.Add(cell);
            cell += direction;
        }
    }
}
