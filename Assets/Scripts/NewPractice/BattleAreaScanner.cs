
using System.Collections.Generic;
using UnityEngine;

public class BattleAreaScanner : MonoBehaviour
{
    [Header("연동")]
    [SerializeField] private BattleAreaMask battleAreaMask;
    [SerializeField] private MapScanner mapScanner;


public int ScanBattleArea()
{
    if (battleAreaMask == null || mapScanner == null)
    {
        Debug.LogError("[BattleAreaScanner] 참조가 연결되지 않았습니다.");
        return 0;
    }

    Vector3Int playerCell =
        battleAreaMask.groundTilemap.WorldToCell(
            battleAreaMask.player.position);

    int leftOffset = (battleAreaMask.areaWidth - 1) / 2;
    int bottomOffset = (battleAreaMask.areaHeight - 1) / 2;

    Vector3Int minCell = new Vector3Int(
        playerCell.x - leftOffset,
        playerCell.y - bottomOffset,
        0
    );

    Vector3Int maxCell = new Vector3Int(
        minCell.x + battleAreaMask.areaWidth,
        minCell.y + battleAreaMask.areaHeight,
        0
    );

    // 1. 전투 영역 안에 있는 적 수집
    List<EnemyControl> battleEnemies = new List<EnemyControl>();

    foreach (var kvp in mapScanner.EnemyPositions)
    {
        EnemyControl enemy = kvp.Value;

        if (enemy == null)
            continue;

        Vector3Int cell = enemy.CurrentCell;

        bool isInside =
            cell.x >= minCell.x && cell.x < maxCell.x &&
            cell.y >= minCell.y && cell.y < maxCell.y;

        if (isInside)
            battleEnemies.Add(enemy);
    }

    // 2. 적 수 출력
    Debug.Log($"========== 전투 구역 스캔 ==========");
    Debug.Log($"전투 구역 내 적: 총 {battleEnemies.Count}명");

    if (battleEnemies.Count == 0)
    {
        Debug.Log("전투 구역에 적이 없습니다.");
        Debug.Log($"==================================");
        return 0;
    }

    // 3. 적 이름 출력
    string enemyNames = "";

    foreach (EnemyControl enemy in battleEnemies)
    {
        enemyNames += enemy.name + ", ";
    }

    enemyNames = enemyNames.TrimEnd(',', ' ');

    Debug.Log($"[적 목록] {enemyNames}");

    // 4. Captain 중복 방지를 위한 HashSet
    HashSet<EnemyCaptainControl> printedCaptains =
        new HashSet<EnemyCaptainControl>();

    foreach (EnemyControl enemy in battleEnemies)
    {
        EnemyCaptainControl captain;

        // 현재 적이 Captain 본인인 경우
        if (enemy is EnemyCaptainControl ownCaptain)
        {
            captain = ownCaptain;
        }
        // 일반 대원인 경우 소속 Captain 확인
        else
        {
            captain = enemy.Captain;
        }

        // 소속 Captain이 없는 독립 대원
        if (captain == null)
        {
            Debug.Log($"[Captain 정보] {enemy.name} → 소속 Captain 없음");
            continue;
        }

        // 이미 출력한 Captain이면 건너뜀
        if (!printedCaptains.Add(captain))
            continue;

        // Captain 본인인지, 해당 Captain의 부하인지 구분
        if (enemy == captain)
        {
            Debug.Log($"[Captain 정보] {captain.name} → 본인이 Captain");
        }
        else
        {
            Debug.Log($"[Captain 정보] {captain.name}");
        }
    }

    Debug.Log($"==================================");

    return battleEnemies.Count;
}


    // 적이 소속된 Captain과 해당 Captain의 부하 목록 출력
    private void PrintCaptainInfo(EnemyControl enemy)
    {
        // 현재 적이 Captain 본인인지 확인
        EnemyCaptainControl captain = enemy as EnemyCaptainControl;

        // Captain 본인이 아니라면 지정된 Captain을 가져옴
        if (captain == null)
            captain = enemy.Captain;

        // Captain이 없는 독립 대원이라면 종료
        if (captain == null)
        {
            Debug.Log(
                $"[{enemy.name}] 소속된 Captain이 없습니다."
            );

            return;
        }

        Debug.Log(
            $"[{captain.name}] Captain 정보"
        );

        // EnemyCaptainControl이 관리하는 실제 부하 목록 사용
        IReadOnlyList<EnemyControl> followers = captain.Followers;

        if (followers.Count == 0)
        {
            Debug.Log("부하가 없습니다.");
            return;
        }

        Debug.Log($"[{captain.name}]의 부하 목록:");

        foreach (EnemyControl follower in followers)
        {
            if (follower == null)
                continue;

            Debug.Log($"- {follower.name}");
        }

        Debug.Log($"총 부하 수: {followers.Count}명");
    }
}
