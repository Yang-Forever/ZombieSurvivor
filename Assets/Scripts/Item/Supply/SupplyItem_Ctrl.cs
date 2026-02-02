using UnityEngine;

public enum SupplyItemType
{
    HpPotion,
    Orbitable
}

public class SupplyItem_Ctrl : MonoBehaviour
{
    public SupplyItemType itemType;
    public int healAmount = 50;

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

    void Heal(Player_Ctrl player)
    {
        PlayerStats ps = PlayerStats.Inst;
        ps.curHp = Mathf.Min(ps.curHp + healAmount, ps.MaxHp);
    }

    void GiveOrbitable()
    {
        OrbitalStrikeSkill.Inst.AddCharge(1);
    }

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
