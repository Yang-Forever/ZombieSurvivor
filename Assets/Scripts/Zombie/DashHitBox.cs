using UnityEngine;

/// <summary>
/// 보스 돌진 공격 중 플레이어 및 벽 충돌을 감지
/// </summary>
public class DashHitBox : MonoBehaviour
{
    Zombie_Ctrl boss;
    bool isHit = false;

    // 보스 참조 설정 및 히트박스 비활성화
    void Start()
    {
        boss = GetComponentInParent<Zombie_Ctrl>();
        gameObject.SetActive(false);
    }

    // 히트박스 활성화 시 피격 상태 초기화
    private void OnEnable()
    {
        isHit = false;
    }

    // 충돌 대상에 따라 대시 종료 또는 데미지 처리
    void OnTriggerEnter(Collider coll)
    {
        if (coll.CompareTag("Wall"))
        {
            boss.EndDash();
            return;
        }

        if (isHit)
            return;

        if (coll.CompareTag("Player"))
        {
            Player_Ctrl player = coll.GetComponent<Player_Ctrl>();
            if (player != null)
            {
                player.HitDamage(40);
                isHit = true;
            }
        }
    }
}
