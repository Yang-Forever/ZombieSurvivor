using UnityEngine;

public abstract class SubWeaponBase : MonoBehaviour
{
    protected ItemRuntimeData data;

    float cooldownTimer = 0f;

    public ItemRuntimeData Data => data;

    public virtual void Init(ItemRuntimeData runtimeData)
    {
        data = runtimeData;
        cooldownTimer = 0f;
    }

    protected virtual void Update()
    {
        if (data == null)
            return;

        cooldownTimer -= Time.deltaTime;
        if (cooldownTimer <= 0f)
        {
            Use();
        }
    }

    protected void ResetCooldown()
    {
        cooldownTimer = Mathf.Max(0.1f, data.GetCoolTime());
    }

    public abstract void Use();

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
