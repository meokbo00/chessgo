using UnityEngine;
using UnityEngine.Tilemaps;

public class InteractableObject : MonoBehaviour
{
    [Header("상호작용 설정")]
    [SerializeField] private string interactionMessage = "무언가를 발견했다!";

    [Header("점유하는 타일 칸 수")]
    [Min(1)]
    [SerializeField] private int widthInCells = 1;

    [Min(1)]
    [SerializeField] private int heightInCells = 1;

    [Header("이동 설정. true면 이동 불가")]
    [SerializeField] private bool isBlock = true;

    [Header("상호작용 여부")]
    [SerializeField] private bool isInteractable = true;

    [Header("점유 칸 시각화")]
    [SerializeField] private Tilemap floorTilemap;
    [SerializeField] private bool showOccupiedCells = true;
    [SerializeField] private Color occupiedColor =
        new Color(1f, 0.2f, 0.2f, 0.25f);

    public int WidthInCells => widthInCells;
    public int HeightInCells => heightInCells;
    public bool IsBlock => isBlock;
    public bool IsInteractable => isInteractable;

    public void Interact()
    {
        if (!isInteractable)
            return;
            
        Debug.Log(interactionMessage);
    }

    private void OnDrawGizmos()
    {
        if (!showOccupiedCells)
            return;

        Vector3 cellSize;
        Vector3 startPosition;

        if (floorTilemap != null)
        {
            // 실제 타일맵의 좌표와 셀 크기 사용
            Vector3Int anchorCell =
                floorTilemap.WorldToCell(transform.position);

            startPosition = floorTilemap
                .GetCellCenterWorld(anchorCell);

            cellSize = floorTilemap.layoutGrid.cellSize;
        }
        else
        {
            // 타일맵 미연결 시 1 유닛 크기로 표시
            cellSize = Vector3.one;
            startPosition = new Vector3(
                Mathf.Floor(transform.position.x) + 0.5f,
                Mathf.Floor(transform.position.y) + 0.5f,
                transform.position.z
            );
        }

        // 기준 칸 중심에서 전체 점유 영역의 중심 계산
        Vector3 totalSize = new Vector3(
            cellSize.x * widthInCells,
            cellSize.y * heightInCells,
            0.05f
        );

        Vector3 center = startPosition + new Vector3(
            (widthInCells - 1) * cellSize.x * 0.5f,
            (heightInCells - 1) * cellSize.y * 0.5f,
            0f
        );

        // 전체 점유 영역 반투명 색상 표시
        Gizmos.color = occupiedColor;
        Gizmos.DrawCube(center, totalSize);

        // 각 칸의 경계를 개별적으로 표시
        Gizmos.color = new Color(
            occupiedColor.r,
            occupiedColor.g,
            occupiedColor.b,
            1f
        );

        for (int x = 0; x < widthInCells; x++)
        {
            for (int y = 0; y < heightInCells; y++)
            {
                Vector3 cellCenter = startPosition + new Vector3(
                    x * cellSize.x,
                    y * cellSize.y,
                    0f
                );

                Gizmos.DrawWireCube(
                    cellCenter,
                    new Vector3(
                        cellSize.x,
                        cellSize.y,
                        0.05f
                    )
                );
            }
        }
    }
}