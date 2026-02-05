using UnityEngine;

public enum SupplyItemType
{
    HpPotion,
    Orbitable
}

/// <summary>
/// 플레이어와 충돌 시 즉시 효과 적용
/// 체력회복 / 오비탈 스킬 충전
/// </summary>
public class SupplyItem_Ctrl : MonoBehaviour
{
    public SupplyItemType itemType;
    public int healAmount = 50;

    // 아이템 타입에 따라 효과 적용
    void ApplyItem(Player_Ctrl player)
    {
        switch(itemType)
        {
            case SupplyItemType.HpPotion:
                Heal(player);
                break;
                case SupplyItemType.Orbitable:
                GiveOrbitable();
                break;
        }
    }

    // 플레이어 체력 회복 처리
    void Heal(Player_Ctrl player)
    {
        PlayerStats ps = PlayerStats.Inst;

        // 최대 체력을 초과하지 않도록 보정
        ps.curHp = Mathf.Min(ps.curHp + healAmount, ps.MaxHp);
        Player_Ctrl.Inst.UpdateHpUI();
    }

    // 오비탈 스킬 충전
    void GiveOrbitable()
    {
        OrbitalStrikeSkill.Inst.AddCharge(1);
    }

    // 플레이어와 충돌 시 아이템 획득 처리
    private void OnTriggerEnter(Collider coll)
    {
        if(coll.CompareTag("Player"))
        {
            Player_Ctrl player = coll.GetComponent<Player_Ctrl>();
            if (player == null)
                return;

            ApplyItem(player);
            Destroy(gameObject);
        }
    }
}
