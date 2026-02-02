using System.Reflection;
using UnityEngine;

public class SubWeaponKnife : SubWeaponBase
{
    public LayerMask zombieLayer;
    public GameObject knifeEffect;
    public Transform playerTr;

    Collider[] hit = new Collider[100];

    private void Awake()
    {
        Player_Ctrl player = FindObjectOfType<Player_Ctrl>();
        if (player != null)
            playerTr = player.transform;
    }

    public override void Use()
    {
        if (data == null)
            return;

        int level = data.curLevel - 1;

        float radius = data.GetLength();
        float damage = data.baseData.baseDamage * data.GetDamageRatio() * PlayerStats.Inst.DamageMultiplier;

        int hitCount = Physics.OverlapSphereNonAlloc(playerTr.position, radius, hit, zombieLayer);

        if (hitCount <= 0)
            return;

        Sound_Mgr.Inst.PlayEffSoundLimit("Knife", 0.7f, 0.1f);

        // ÀÌÆåÆ®
        GameObject effect = Instantiate(knifeEffect, playerTr.position, Quaternion.identity, transform);
        Destroy(effect, 1f);

        for (int i = 0; i < hitCount; i++)
        {
            Zombie_Ctrl zombie = hit[i].GetComponentInParent<Zombie_Ctrl>();
            if (zombie != null && !zombie.isDead)
            {
                zombie.HitDamage(damage);
            }
        }

        ResetCooldown();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(playerTr.position, data.GetLength());
    }
}
