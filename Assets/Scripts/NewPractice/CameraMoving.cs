
using UnityEngine;
using Unity.Cinemachine;

[RequireComponent(typeof(CinemachineCamera))]
public class CameraMoving : MonoBehaviour
{
    [Header("플레이어 설정")]
    [SerializeField] private string playerTag = "Player";

    [Header("데드존 설정")]
    [Range(0f, 1f)]
    [SerializeField] private float deadZoneWidth = 0.3f;

    [Range(0f, 1f)]
    [SerializeField] private float deadZoneHeight = 0.3f;

    [Header("카메라 부드러움 설정")]
    [SerializeField] private float dampingX = 0.5f;
    [SerializeField] private float dampingY = 0.5f;

    private CinemachineCamera cinemachineCamera;
    private CinemachinePositionComposer positionComposer;

    private Transform playerTarget;

    private void Awake()
    {
        cinemachineCamera = GetComponent<CinemachineCamera>();

        positionComposer =
            GetComponent<CinemachinePositionComposer>();

        if (positionComposer == null)
        {
            Debug.LogError(
                "Cinemachine Position Composer가 없습니다!"
            );
        }
    }

    private void Update()
    {
        // 아직 플레이어를 찾지 못했다면 계속 검색
        if (playerTarget == null)
        {
            FindPlayer();
        }
    }

    private void FindPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag(playerTag);

        if (player == null)
            return;

        playerTarget = player.transform;

        // Cinemachine의 추적 대상 지정
        cinemachineCamera.Follow = playerTarget;

        // 카메라 세부 설정
        SetCameraSettings();

        Debug.Log("플레이어 카메라 추적 시작!");
    }

    private void SetCameraSettings()
    {
        if (positionComposer == null)
            return;

        // 카메라 이동 부드러움
        positionComposer.Damping = new Vector3(
            dampingX,
            dampingY,
            0f
        );

        // 데드존 설정
        var composition = positionComposer.Composition;

        var deadZone = composition.DeadZone;

        deadZone.Enabled = true;

        deadZone.Size = new Vector2(
            deadZoneWidth,
            deadZoneHeight
        );

        composition.DeadZone = deadZone;

        // 데드존의 중심을 화면 정중앙으로 설정
        composition.ScreenPosition = Vector2.zero;

        positionComposer.Composition = composition;

        // 카메라 활성화 시 무조건 중앙으로 이동하는 동작 방지
        positionComposer.CenterOnActivate = false;
    }
}
