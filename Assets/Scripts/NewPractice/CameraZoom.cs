
using UnityEngine;
using Unity.Cinemachine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CinemachineCamera))]
public class CameraZoom : MonoBehaviour
{
    [Header("줌 설정")]
    [SerializeField] private float zoomSpeed = 2f;

    [SerializeField] private float minZoom = 3f;
    [SerializeField] private float maxZoom = 12f;

    [Header("줌 부드러움")]
    [SerializeField] private float smoothSpeed = 5f;

    private CinemachineCamera cinemachineCamera;

    private float targetZoom;
    private float zoomVelocity;

    private void Awake()
    {
        cinemachineCamera = GetComponent<CinemachineCamera>();

        targetZoom = cinemachineCamera.Lens.OrthographicSize;
    }

    private void Update()
    {
        // 마우스 휠 입력 확인
        if (Mouse.current != null)
        {
            float scroll = Mouse.current.scroll.ReadValue().y / 120f;

            if (scroll != 0)
            {
                // 휠 위로: 줌 인
                // 휠 아래로: 줌 아웃
                targetZoom -= scroll * zoomSpeed;

                // 줌 범위 제한
                targetZoom = Mathf.Clamp(
                    targetZoom,
                    minZoom,
                    maxZoom
                );
            }
        }

        // 부드러운 줌 처리
        var lens = cinemachineCamera.Lens;

        float currentZoom = lens.OrthographicSize;

        lens.OrthographicSize = Mathf.SmoothDamp(
            currentZoom,
            targetZoom,
            ref zoomVelocity,
            1f / smoothSpeed
        );

        cinemachineCamera.Lens = lens;
    }
}
