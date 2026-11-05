
using UnityEngine;
using UnityEngine.Tilemaps;

public class BattleAreaMask : MonoBehaviour
{
    [Header("기본 설정")]
    public Transform player;
    public Tilemap groundTilemap;
    public Camera mainCamera;
    public Canvas canvas;

    [Header("검은색 패널 4개")]
    public RectTransform blackTop;
    public RectTransform blackBottom;
    public RectTransform blackLeft;
    public RectTransform blackRight;

    [Header("전투 영역")]
    public int areaWidth = 5;
    public int areaHeight = 5;

    [Header("애니메이션")]
    public float transitionDuration = 0.4f;

    private RectTransform canvasRect;

    private float progress = 0f;
    private bool targetVisible = false;

    void Awake()
    {
        canvasRect = canvas.GetComponent<RectTransform>();

        SetPanelsActive(false);
    }

    // 전투 시작 시 호출
    public void Show()
    {
        targetVisible = true;
        SetPanelsActive(true);
    }

    // 전투 종료 시 호출
    public void Hide()
    {
        targetVisible = false;
    }

    void LateUpdate()
    {
        if (targetVisible || progress > 0f)
        {
            // 애니메이션 진행
            float target = targetVisible ? 1f : 0f;

            progress = Mathf.MoveTowards(
                progress,
                target,
                Time.unscaledDeltaTime /
                Mathf.Max(transitionDuration, 0.01f)
            );

            // 부드러운 이동
            float easedProgress =
                Mathf.SmoothStep(0f, 1f, progress);

            // 현재 플레이어와 카메라 기준으로 위치 계산
            UpdateMask(easedProgress);

            // 퇴장 애니메이션이 끝나면 비활성화
            if (!targetVisible && progress <= 0f)
            {
                SetPanelsActive(false);
            }
        }
    }

    void UpdateMask(float animationProgress)
    {
        if (player == null ||
            groundTilemap == null ||
            mainCamera == null ||
            canvasRect == null)
            return;

        if (!TryGetAreaBounds(out Vector3Int minCell, out Vector3Int maxCell))
            return;

        // 3. Tilemap 기준 월드 좌표
        Vector3 worldBottomLeft =
            groundTilemap.CellToWorld(minCell);

        Vector3 worldTopRight =
            groundTilemap.CellToWorld(maxCell);

        // 4. 월드 좌표를 화면 좌표로 변환
        Vector3 screenBottomLeft =
            mainCamera.WorldToScreenPoint(worldBottomLeft);

        Vector3 screenTopRight =
            mainCamera.WorldToScreenPoint(worldTopRight);

        // 카메라 뒤쪽에 있는 영역은 처리하지 않음
        if (screenBottomLeft.z < 0 ||
            screenTopRight.z < 0)
            return;

        // 5. 화면 경계 안으로 제한
        float left = Mathf.Clamp(
            screenBottomLeft.x, 0, Screen.width);

        float right = Mathf.Clamp(
            screenTopRight.x, 0, Screen.width);

        float bottom = Mathf.Clamp(
            screenBottomLeft.y, 0, Screen.height);

        float top = Mathf.Clamp(
            screenTopRight.y, 0, Screen.height);

        // 6. 애니메이션에 사용할 이동 거리
        float moveX = canvasRect.rect.width *
                      (1f - animationProgress);

        float moveY = canvasRect.rect.height *
                      (1f - animationProgress);

        // 7. 검은색 패널 4개 배치
        SetPanel(
            blackTop,
            new Vector2(0, top),
            new Vector2(Screen.width, Screen.height),
            new Vector2(0, moveY)
        );

        SetPanel(
            blackBottom,
            new Vector2(0, 0),
            new Vector2(Screen.width, bottom),
            new Vector2(0, -moveY)
        );

        SetPanel(
            blackLeft,
            new Vector2(0, bottom),
            new Vector2(left, top),
            new Vector2(-moveX, 0)
        );

        SetPanel(
            blackRight,
            new Vector2(right, bottom),
            new Vector2(Screen.width, top),
            new Vector2(moveX, 0)
        );
    }

    // 화면 좌표를 Canvas의 UI 좌표로 변환
    void SetPanel(
        RectTransform panel,
        Vector2 screenMin,
        Vector2 screenMax,
        Vector2 animationOffset)
    {
        if (panel == null)
            return;

        Vector2 localMin;
        Vector2 localMax;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            screenMin,
            null,
            out localMin
        );

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            screenMax,
            null,
            out localMax
        );

        panel.anchorMin = new Vector2(0.5f, 0.5f);
        panel.anchorMax = new Vector2(0.5f, 0.5f);
        panel.pivot = new Vector2(0.5f, 0.5f);

        panel.sizeDelta = localMax - localMin;

        panel.anchoredPosition =
            (localMin + localMax) * 0.5f
            + animationOffset;
    }

    public bool TryGetAreaBounds(out Vector3Int minCell, out Vector3Int maxCell)
    {
        minCell = default;
        maxCell = default;

        if (player == null || groundTilemap == null)
            return false;

        Vector3Int playerCell =
            groundTilemap.WorldToCell(player.position);

        int leftOffset = (areaWidth - 1) / 2;
        int bottomOffset = (areaHeight - 1) / 2;

        minCell = new Vector3Int(
            playerCell.x - leftOffset,
            playerCell.y - bottomOffset,
            0
        );

        maxCell = new Vector3Int(
            minCell.x + areaWidth,
            minCell.y + areaHeight,
            0
        );

        return true;
    }

    void SetPanelsActive(bool active)
    {
        blackTop.gameObject.SetActive(active);
        blackBottom.gameObject.SetActive(active);
        blackLeft.gameObject.SetActive(active);
        blackRight.gameObject.SetActive(active);
    }
}
