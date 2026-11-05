using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

// 전투 모드를 관리하는 싱글톤 매니저
// 전투 모드 진입 시 모든 EnemyControl을 멈추고, 종료 시 다시 움직이게 함
public class CombatManager : MonoBehaviour
{
    private static CombatManager instance;
    public static CombatManager Instance => instance;

    [Header("전투 모드 설정")]
    [SerializeField] private bool startInCombatMode = false;

    private bool isInCombatMode = false;
    private EnemyControl[] allEnemies;
    private HeroControl heroControl;
    public BattleAreaMask battleAreaMask;
    public BattleAreaScanner battleAreaScanner;
    public Button BattleStartBtn;

    // 원래 전투 영역 크기 저장 (복구용)
    private int originalAreaWidth;
    private int originalAreaHeight;

    // 배치된 Captain과 그 부하들 (활성화 유지용)
    private HashSet<EnemyControl> activeInBattle = new HashSet<EnemyControl>();


    private void Awake()
    {
        // 싱글톤 패턴 구현
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        // 모든 EnemyControl과 HeroControl 찾기
        RefreshEnemyList();
        FindHeroControl();

        // 시작 시 전투 모드 설정
        if (startInCombatMode)
            EnterCombatMode();
    }

    private void Start()
    {
        // Enemy들이 스폰된 후 다시 한번 리스트 갱신
        RefreshEnemyList();
        FindHeroControl();
    }

    // 전투 모드 토글
    public void ToggleCombatMode()
    {
        if (isInCombatMode)
            ExitCombatMode();
        else
            EnterCombatMode();
    }

    // 전투 모드 진입하는 최초 메서드!!!!
    public void EnterCombatMode()
    {
        if (isInCombatMode)
            return;

        isInCombatMode = true;
        Debug.Log("[CombatManager] 전투 모드 진입");

        // 원래 영역 크기 저장
        if (battleAreaMask != null)
        {
            originalAreaWidth = battleAreaMask.areaWidth;
            originalAreaHeight = battleAreaMask.areaHeight;
        }

        // 모든 Enemy 멈추기
        StopAllEnemies();
        battleAreaMask.Show();
        battleAreaScanner.ScanBattleArea();
        if (battleAreaScanner.battleEnemies.Count > 0)
        {
            BattleStartBtn.gameObject.SetActive(true);
        }
    }

    // 전투 모드 종료
    public void ExitCombatMode()
    {
        if (!isInCombatMode)
            return;

        isInCombatMode = false;
        Debug.Log("[CombatManager] 전투 모드 종료");

        // 원래 영역 크기 복구
        if (battleAreaMask != null)
        {
            battleAreaMask.areaWidth = originalAreaWidth;
            battleAreaMask.areaHeight = originalAreaHeight;
        }

        RestoreEnemyOverworldPositions();

        // 모든 Enemy 재개 및 활성화
        ResumeAllEnemies();
        EnableAllEnemies();
        battleAreaMask.Hide();
        BattleStartBtn.gameObject.SetActive(false);
    }

    // 모든 EnemyControl 리스트 갱신 (새로 스폰된 Enemy 포함)
    public void RefreshEnemyList()
    {
        allEnemies = FindObjectsByType<EnemyControl>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        Debug.Log($"[CombatManager] 현재 적 {allEnemies.Length}명 발견");
    }

    // HeroControl 찾기
    private void FindHeroControl()
    {
        heroControl = FindFirstObjectByType<HeroControl>();
    }

    // 모든 Enemy와 Hero 멈추기
    private void StopAllEnemies()
    {
        foreach (var enemy in allEnemies)
        {
            if (enemy != null)
                enemy.SetPaused(true);
        }

        // Hero도 멈추기
        if (heroControl != null)
            heroControl.SetPaused(true);
    }

    private void RestoreEnemyOverworldPositions()
    {
        if (allEnemies == null)
            return;

        foreach (var enemy in allEnemies)
        {
            if (enemy != null)
                enemy.RestoreOverworldCell();
        }
    }

    // 모든 Enemy와 Hero 재개
    private void ResumeAllEnemies()
    {
        foreach (var enemy in allEnemies)
        {
            if (enemy != null)
                enemy.SetPaused(false);
        }

        // Hero도 재개
        if (heroControl != null)
            heroControl.SetPaused(false);
    }

    public void OnClickBattleScanBtn()
    {
        // 전투 감지 로직
        Debug.Log("[CombatManager] 전투 감지 버튼 클릭");

        // 스캔된 Captain 정보 받아오기
        if (battleAreaScanner != null)
        {
            var captains = battleAreaScanner.GetCaptains();

            if (captains.Count > 0)
            {
                // 첫 번째 Captain의 크기로 전투 영역 설정
                var firstCaptain = captains[0];
                int newWidth = firstCaptain.width;
                int newHeight = firstCaptain.height;

                Debug.Log($"Captain: {firstCaptain.name}, Width: {newWidth}, Height: {newHeight}");

                // BattleAreaMask의 영역 크기 변경
                if (battleAreaMask != null)
                {
                    battleAreaMask.areaWidth = newWidth;
                    battleAreaMask.areaHeight = newHeight;

                    // 전투 영역 다시 표시
                    battleAreaMask.Hide();
                    battleAreaMask.Show();

                    Debug.Log($"전투 영역이 {newWidth}x{newHeight}로 변경되었습니다.");

                    if (battleAreaMask.TryGetAreaBounds(out Vector3Int minCell, out _))
                    {
                        // 배치된 Captain과 부하들을 활성화 유지 목록에 추가
                        activeInBattle.Clear();
                        activeInBattle.Add(firstCaptain);
                        foreach (var follower in firstCaptain.Followers)
                        {
                            if (follower != null)
                                activeInBattle.Add(follower);
                        }

                        // 배치 실행
                        firstCaptain.ApplyFormation(minCell);

                        // 나머지 Enemy 비활성화
                        DisableOtherEnemies();
                    }
                }
            }
            else
            {
                Debug.Log("Captain이 없습니다.");
            }
        }
    }

    // 배치되지 않은 Enemy 비활성화
    private void DisableOtherEnemies()
    {
        foreach (var enemy in allEnemies)
        {
            if (enemy == null)
                continue;

            // 배치된 Captain이나 부하가 아니면 비활성화
            if (!activeInBattle.Contains(enemy))
            {
                enemy.gameObject.SetActive(false);
            }
        }
    }

    // 모든 Enemy 다시 활성화
    private void EnableAllEnemies()
    {
        foreach (var enemy in allEnemies)
        {
            if (enemy != null)
            {
                enemy.gameObject.SetActive(true);
            }
        }

        activeInBattle.Clear();
    }

    // 현재 전투 모드인지 확인
    public bool IsInCombatMode => isInCombatMode;
}
