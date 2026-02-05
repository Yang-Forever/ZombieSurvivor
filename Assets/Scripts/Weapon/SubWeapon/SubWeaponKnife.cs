using UnityEngine;

/// <summary>
/// 플레이어 주변 범위 내 적을 공격하는 서브 무기
/// </summary>
public class SubWeaponKnife : SubWeaponBase
{
    public LayerMask zombieLayer;
    public GameObject knifeEffect;
    public Transform playerTr;

    Collider[] hit = new Collider[100];

    // 플레이어 트랜스폼 참조 설정
    private void Awake()
    {
        Player_Ctrl player = FindObjectOfType<Player_Ctrl>();
        if (player != null)
            playerTr = player.transform;
    }

    // 나이프 공격 실행
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

        // 이펙트
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

    // 공격 범위 기즈모 표시
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(playerTr.position, data.GetLength());
    }
}
