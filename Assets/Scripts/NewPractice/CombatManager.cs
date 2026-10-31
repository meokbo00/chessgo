using UnityEngine;

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

        // 모든 EnemyControl 찾기
        RefreshEnemyList();

        // 시작 시 전투 모드 설정
        if (startInCombatMode)
            EnterCombatMode();
    }

    private void Start()
    {
        // Enemy들이 스폰된 후 다시 한번 리스트 갱신
        RefreshEnemyList();
    }

    // 전투 모드 토글
    public void ToggleCombatMode()
    {
        if (isInCombatMode)
            ExitCombatMode();
        else
            EnterCombatMode();
    }

    // 전투 모드 진입
    public void EnterCombatMode()
    {
        if (isInCombatMode)
            return;

        isInCombatMode = true;
        Debug.Log("[CombatManager] 전투 모드 진입");

        // 모든 Enemy 멈추기
        StopAllEnemies();
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
    }

    // 모든 EnemyControl 리스트 갱신 (새로 스폰된 Enemy 포함)
    public void RefreshEnemyList()
    {
        allEnemies = FindObjectsByType<EnemyControl>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        Debug.Log($"[CombatManager] 현재 적 {allEnemies.Length}명 발견");
    }

    // 모든 Enemy 멈추기
    private void StopAllEnemies()
    {
        foreach (var enemy in allEnemies)
        {
            if (enemy != null)
                enemy.SetPaused(true);
        }
    }

    // 모든 Enemy 재개
    private void ResumeAllEnemies()
    {
        foreach (var enemy in allEnemies)
        {
            if (enemy != null)
                enemy.SetPaused(false);
        }
    }

    // 현재 전투 모드인지 확인
    public bool IsInCombatMode => isInCombatMode;
}
