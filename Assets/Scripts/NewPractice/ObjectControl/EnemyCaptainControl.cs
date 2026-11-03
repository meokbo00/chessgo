
using System.Collections.Generic;
using UnityEngine;

public class EnemyCaptainControl : EnemyControl
{
    [Header("Captain의 대원 목록")]
    [SerializeField] private List<EnemyControl> followers = new List<EnemyControl>();

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
}
