using UnityEngine;

/// <summary>
/// 폭발 범위 내의 적에게 데미지를 주고
/// 일정 시간 후 오브젝트를 제거하는 폭발
/// </summary>
public class Explosion_Ctrl : MonoBehaviour
{
    public float radius = 8f;
    public float damage = 100;
    public float lifeTime = 1.5f;

    // 폭발 범위 내 적에게 데미지 적용
    void Start()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, radius, LayerMask.GetMask("NormalZombie", "ExplosionZombie", "BossZombie"));

        foreach (var col in hits)
        {
            float dmg = damage * PlayerStats.Inst.DamageMultiplier;

            Zombie_Ctrl z = col.GetComponent<Zombie_Ctrl>();
            if (z)
                z.HitDamage(dmg);
        }

        Destroy(gameObject, lifeTime);
    }
}
