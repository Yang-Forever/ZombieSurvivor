using UnityEngine;

public class Explosion_Ctrl : MonoBehaviour
{
    public float radius = 8f;
    public float damage = 100;
    public float lifeTime = 1.5f;

    // Start is called before the first frame update
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
