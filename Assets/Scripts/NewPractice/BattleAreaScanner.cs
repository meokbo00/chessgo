
using System.Collections.Generic;
using UnityEngine;

public class BattleAreaScanner : MonoBehaviour
{
    [Header("연동")]
    [SerializeField] private BattleAreaMask battleAreaMask;
    [SerializeField] private MapScanner mapScanner;

    // 전투 영역 안에 있는 적 목록 (외부 접근용)
    public List<EnemyControl> battleEnemies = new List<EnemyControl>();


    public int ScanBattleArea()
    {
        // 0. 참조 확인
        if (battleAreaMask == null || mapScanner == null)
        {
            Debug.LogError("[BattleAreaScanner] 참조가 연결되지 않았습니다.");
            return 0;
        }

        // 1. 플레이어의 현재 셀 확인
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


        // 2. 전투 영역 안에 있는 적 수집
        battleEnemies.Clear();

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
            {
                battleEnemies.Add(enemy);
            }
        }


        // 3. 적 수 출력
        Debug.Log("========== 전투 구역 스캔 ==========");
        Debug.Log($"전투 구역 내 적: 총 {battleEnemies.Count}명");

        if (battleEnemies.Count == 0)
        {
            Debug.Log("전투 구역에 적이 없습니다.");
            Debug.Log("==================================");
            return 0;
        }


        // 4. 적 이름 출력
        string enemyNames = "";

        foreach (EnemyControl enemy in battleEnemies)
        {
            enemyNames += enemy.name + ", ";
        }

        enemyNames = enemyNames.TrimEnd(',', ' ');

        Debug.Log($"[적 목록] {enemyNames}");


        // 5. Captain 정보 수집
        List<EnemyCaptainControl> captains =
            new List<EnemyCaptainControl>();

        // 이미 수집한 Captain 중복 방지
        HashSet<EnemyCaptainControl> seenCaptains =
            new HashSet<EnemyCaptainControl>();

        // 전투 영역 안에 Captain 본인이 있는지 확인
        HashSet<EnemyCaptainControl> captainsInArea =
            new HashSet<EnemyCaptainControl>();

        // 소속 Captain이 없는 독립 적
        List<EnemyControl> independentEnemies =
            new List<EnemyControl>();

        // 캡틴 목록을 저장할 리스트 (외부 반환용)
        List<EnemyCaptainControl> captainList = new List<EnemyCaptainControl>();


        foreach (EnemyControl enemy in battleEnemies)
        {
            EnemyCaptainControl captain;

            // 현재 적이 Captain 본인인지 확인
            if (enemy is EnemyCaptainControl ownCaptain)
            {
                captain = ownCaptain;

                // Captain 본인이 전투 영역 안에 있음
                captainsInArea.Add(ownCaptain);
            }
            else
            {
                // 일반 대원의 소속 Captain 확인
                captain = enemy.Captain;
            }


            // 소속 Captain이 없는 독립 적
            if (captain == null)
            {
                independentEnemies.Add(enemy);
                continue;
            }


            // Captain 중복 없이 목록에 추가
            if (seenCaptains.Add(captain))
            {
                captains.Add(captain);
                captainList.Add(captain);
            }
        }


        // 6. Captain 정보 출력
        foreach (EnemyCaptainControl captain in captains)
        {
            if (captainsInArea.Contains(captain))
            {
                Debug.Log(
                    $"[Captain 정보] {captain.name} → 본인이 Captain");
            }
            else
            {
                Debug.Log(
                    $"[Captain 정보] 감지된 적의 소속 캡틴: {captain.name}");
            }
        }


        // 7. 소속 Captain이 없는 적 출력
        foreach (EnemyControl enemy in independentEnemies)
        {
            Debug.Log(
                $"[Captain 정보] {enemy.name} → 소속 Captain 없음");
        }


        // 8. 스캔 종료
        Debug.Log("==================================");

        return battleEnemies.Count;
    }

    // 스캔된 Captain 목록 반환 (combatManaer에서 사용!!)
    public List<EnemyCaptainControl> GetCaptains()
    {
        List<EnemyCaptainControl> captains = new List<EnemyCaptainControl>();

        // 이미 수집한 Captain 중복 방지
        HashSet<EnemyCaptainControl> seenCaptains =
            new HashSet<EnemyCaptainControl>();

        foreach (EnemyControl enemy in battleEnemies)
        {
            EnemyCaptainControl captain;

            // 현재 적이 Captain 본인인지 확인
            if (enemy is EnemyCaptainControl ownCaptain)
            {
                captain = ownCaptain;
            }
            else
            {
                // 일반 대원의 소속 Captain 확인
                captain = enemy.Captain;
            }

            // 소속 Captain이 없는 독립 적은 건너뜀
            if (captain == null)
                continue;

            // Captain 중복 없이 목록에 추가
            if (seenCaptains.Add(captain))
            {
                captains.Add(captain);
            }
        }

        return captains;
    }


    // 적이 소속된 Captain과 해당 Captain의 부하 목록 출력
    private void PrintCaptainInfo(EnemyControl enemy)
    {
        // 현재 적이 Captain 본인인지 확인
        EnemyCaptainControl captain = enemy as EnemyCaptainControl;

        // Captain 본인이 아니라면 지정된 Captain을 가져옴
        if (captain == null)
        {
            captain = enemy.Captain;
        }

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
