using UnityEngine;


/// <summary>
/// 모든 서브 무기의 공통 동작과
/// 쿨타임 처리 구조를 정의하는 추상 베이스 클래스
/// </summary>
public abstract class SubWeaponBase : MonoBehaviour
{
    protected ItemRuntimeData data;

    [HideInInspector] public float cooldownTimer = 0f;

    public ItemRuntimeData Data => data;

    // 서브 무기 초기화
    public virtual void Init(ItemRuntimeData runtimeData)
    {
        data = runtimeData;
        cooldownTimer = 0f;
    }


    // 쿨타임 감소 및 사용 타이밍 처리
    protected virtual void Update()
    {
        if (data == null)
            return;

        if (cooldownTimer > 0f)
        {
            cooldownTimer -= Time.deltaTime;
            return;
        }

        Use();
    }

    // 쿨타임 초기화
    protected void ResetCooldown()
    {
        cooldownTimer = Mathf.Max(0.1f, data.GetCoolTime());
    }

    // 각 서브 무기별 사용 로직
    public abstract void Use();

    // 현재 쿨타임 비율 반환
    public float GetCooldownRatio()
    {
        if (data == null)
            return 0f;

        float total = data.GetCoolTime();

        if (total <= 0f)
            return 0f;

        return Mathf.Clamp01(cooldownTimer / total);
    }
}
