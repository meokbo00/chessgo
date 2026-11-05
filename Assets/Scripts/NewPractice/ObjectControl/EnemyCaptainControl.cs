using System;
using System.Collections.Generic;
using UnityEngine;

public class EnemyCaptainControl : EnemyControl
{
    [Header("실제 전투 영역 크기")]
    public int width = 9;
    public int height = 9;


    [Header("Captain의 대원 목록")]
    [SerializeField] private List<EnemyControl> followers = new List<EnemyControl>();

    [Header("전투 배치 (로컬 칸)")]
    [Tooltip("칸 (0,0)은 전투 영역의 왼쪽 아래입니다. X는 오른쪽, Y는 위쪽입니다.")]
    [SerializeField] private List<FormationSlot> formation = new List<FormationSlot>();


    public IReadOnlyList<FormationSlot> Formation => formation;

    // 외부 스크립트에서 부하 목록 확인
    public IReadOnlyList<EnemyControl> Followers => followers;

    // 현재 부하 수 확인
    public int FollowerCount => followers.Count;


    // 새로운 부하 등록
    public void RegisterFollower(EnemyControl follower)
    {
        if (follower == null)
            return;

        // Captain 본인은 부하로 등록하지 않음
        if (follower == this)
            return;

        // 이미 등록된 부하는 중복 등록하지 않음
        if (followers.Contains(follower))
            return;

        followers.Add(follower);

        Debug.Log(
            $"[{name}] 부하 등록 완료: {follower.name}"
        );
    }


    // 부하 등록 해제
    public void UnregisterFollower(EnemyControl follower)
    {
        if (follower == null)
            return;

        if (followers.Remove(follower))
        {
            Debug.Log(
                $"[{name}] 부하 등록 해제: {follower.name}"
            );
        }
    }

    // 전투 영역 왼쪽 아래를 원점으로, 미리 지정한 칸에 캡틴/대원을 배치한다.
    public void ApplyFormation(Vector3Int areaMinCell)
    {
        if (formation == null || formation.Count == 0)
            return;

        foreach (FormationSlot slot in formation)
        {
            EnemyControl unit = slot.GetUnit(this);
            if (unit == null)
                continue;

            if (!IsInsideFormation(slot.x, slot.y))
            {
                Debug.LogWarning(
                    $"[{name}] 배치 칸이 전투 영역을 벗어났습니다: ({slot.x}, {slot.y})"
                );
                continue;
            }

            Vector3Int worldCell = new Vector3Int(
                areaMinCell.x + slot.x,
                areaMinCell.y + slot.y,
                0
            );

            unit.PlaceAtCellForBattle(worldCell);
        }
    }

    private bool IsInsideFormation(int x, int y)
    {
        return x >= 0 && x < width && y >= 0 && y < height;
    }

    private void OnValidate()
    {
        if (width < 1)
            width = 1;
        if (height < 1)
            height = 1;

        if (formation == null)
            return;

        HashSet<Vector2Int> usedCells = new HashSet<Vector2Int>();

        foreach (FormationSlot slot in formation)
        {
            slot.x = Mathf.Clamp(slot.x, 0, Mathf.Max(0, width - 1));
            slot.y = Mathf.Clamp(slot.y, 0, Mathf.Max(0, height - 1));

            Vector2Int cell = new Vector2Int(slot.x, slot.y);
            if (!usedCells.Add(cell))
            {
                Debug.LogWarning(
                    $"[{name}] 같은 칸에 배치가 중복됩니다: ({slot.x}, {slot.y})"
                );
            }
        }
    }
}

[Serializable]
public class FormationSlot
{
    [Tooltip("이 칸에 둘 유닛입니다. 비워두면 캡틴 본인으로 처리합니다.")]
    public EnemyControl unit;

    public int x;
    public int y;

    public EnemyControl GetUnit(EnemyCaptainControl captain)
    {
        if (unit != null)
            return unit;

        return captain;
    }
}
