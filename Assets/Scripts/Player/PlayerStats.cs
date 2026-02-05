using UnityEngine;

/// <summary>
/// 플레이어의 모든 스탯을 관리하는 클래스
/// 기본 스탯 + 보너스 스탯을 합산하여 최종 값을 제공
/// </summary>
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

    // 최종 공격력 배율
    public float DamageMultiplier => baseDamageMultiplier + bonusDamageMultiplier;

    // 최종 공격 속도
    public float AttackSpeed => baseAttackSpeed + bonusAttackSpeed;

    // 최종 이동 속도
    public float MoveSpeed => baseMoveSpeed + bonusMoveSpeed;

    // 최종 자석 범위
    public float MagnetRange => baseMagnetRange * (1f + bonusMagnetRangeMultiplier);

    // 최종 최대 체력
    public float MaxHp =>
        baseMaxHp + bonusMaxHp;

    // 총알 관통
    public int Penetration =>
        bonusPenetration;

    // 피해 감소
    public float DamageReduction =>
        bonusReduction;

    public static PlayerStats Inst = null;

    private void Awake()
    {
        Inst = this;

        curHp = baseMaxHp;
    }

    // 공격력 배율 증가
    public void AddDmgMultyplier(float value)
    {
        bonusDamageMultiplier += value;
    }

    // 공격 속도 증가
    public void AddAtkSpeed(float value)
    {
        bonusAttackSpeed += value;
    }

    // 이동 속도 증가
    public void AddMvSpeed(float value)
    {
        bonusMoveSpeed += value;
    }

    // 자석 범위 배율 증가
    public void AddMagnetRange(float value)
    {
        bonusMagnetRangeMultiplier += value;
    }

    // 최대 체력 증가 + 증가한 만큼 현재 체력 보정
    public void AddHp(float value)
    {
        float temp = MaxHp;
        bonusMaxHp += value;
        curHp += MaxHp - temp;

        if(curHp >= MaxHp)
            curHp = MaxHp;

        Player_Ctrl.Inst.UpdateHpUI();
    }

    // 피해 감소율 증가
    public void AddReduction(float value)
    {
        bonusReduction += value;
    }

    // 관통 수 증가
    public void AddPenetration(int value)
    {
        bonusPenetration += value;
    }

    // 모든 보너스 스텟 초기화
    public void ResetStats()
    {
        bonusDamageMultiplier = 0f;
        bonusAttackSpeed = 0f;
        bonusMoveSpeed = 0f;
        bonusMagnetRangeMultiplier = 0f;
        bonusMaxHp = 0f;
        bonusReduction = 0f;
        bonusPenetration = 0;
        curHp = baseMaxHp;
    }

}
