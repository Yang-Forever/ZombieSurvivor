using UnityEngine;

/// <summary>
/// 좀비 애니메이션 이벤트를 받아
/// 공격 타이밍과 종료 처리를 연결하는 클래스
/// </summary>
public class ZombieAnimEvent : MonoBehaviour
{
    Zombie_Ctrl zombie;

    // 부모 오브젝트에서 좀비 컨트롤러 참조
    void Start()
    {
        zombie = GetComponentInParent<Zombie_Ctrl>();
    }

    // 공격 히트 타이밍 이벤트 처리
    public void Event_AtkHit()
    {
        if (zombie != null)
            zombie.OnAtkHit();
    }

    // 공격 애니메이션 종료 이벤트 처리
    public void Event_AttackEnd()
    {
        if (zombie != null)
            zombie.OnAttackEnd();
    }
}
