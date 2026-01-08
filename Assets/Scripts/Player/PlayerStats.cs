using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [Header("Base Stats")]
    public float baseDamageMultiplier = 1f;
    public float baseAttackSpeed = 1f;
    public float baseMoveSpeed = 5f;
    public float baseMagnetRange = 3f;
    public float baseMaxHp = 100f;

    [Header("Bonus Stats")]
    public float bonusDamageMultiplier;
    public float bonusAttackSpeed;
    public float bonusMoveSpeed;
    public float bonusMagnetRangeMultiplier;
    public float bonusMaxHp;
    public float bonusReduction;
    public int bonusPenetration;

    [HideInInspector] public float curHp;
    public float DamageMultiplier =>
        baseDamageMultiplier + bonusDamageMultiplier;

    public float AttackSpeed =>
        baseAttackSpeed + bonusAttackSpeed;

    public float MoveSpeed =>
        baseMoveSpeed + bonusMoveSpeed;

    public float MagnetRange =>
        baseMagnetRange * (1f + bonusMagnetRangeMultiplier);

    public float MaxHp =>
        baseMaxHp + bonusMaxHp;

    public int Penetration =>
        bonusPenetration;

    public float DamageReduction =>
        bonusReduction;

    public static PlayerStats Inst = null;

    private void Awake()
    {
        Inst = this;

        curHp = baseMaxHp;
    }

    public void AddDmgMultyplier(float value)
    {
        bonusDamageMultiplier += value;
    }

    public void AddAtkSpeed(float value)
    {
        bonusAttackSpeed += value;
    }

    public void AddMvSpeed(float value)
    {
        bonusMoveSpeed += value;
    }

    public void AddMagnetRange(float value)
    {
        baseMagnetRange += value;
    }

    public void AddHp(float value)
    {
        float temp = MaxHp;
        bonusMaxHp += value;
        curHp += MaxHp - temp;

        if(curHp >= MaxHp)
            curHp = MaxHp;
    }

    public void AddReduction(float value)
    {
        bonusReduction += value;
    }

    public void AddPenetration(int value)
    {
        bonusPenetration += value;
    }

}
