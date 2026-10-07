using UnityEngine;

public enum CellType
{
    Ground,   // 이동 가능한 일반 바닥
    Blocked,  // 벽, 책상 등 이동 불가 장애물
    Cliff     // 절벽 (낙사 지역)
}