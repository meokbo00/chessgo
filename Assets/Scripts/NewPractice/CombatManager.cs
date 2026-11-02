using UnityEngine;
using UnityEngine.UI;
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
    public Button BattleStartBtn;

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

        // 모든 Enemy 멈추기
        StopAllEnemies();
        battleAreaMask.Show();
        BattleStartBtn.gameObject.SetActive(true);
    }

    // 전투 모드 종료
    public void ExitCombatMode()
    {
        if (!isInCombatMode)
            return;

        isInCombatMode = false;
        Debug.Log("[CombatManager] 전투 모드 종료");

        // 모든 Enemy 재개
        ResumeAllEnemies();
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

    // 현재 전투 모드인지 확인
    public bool IsInCombatMode => isInCombatMode;
}
