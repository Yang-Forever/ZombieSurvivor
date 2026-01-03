using UnityEngine;

public abstract class SubWeaponBase : MonoBehaviour
{
    protected ItemRuntimeData data;

    float cooldownTimer = 0f;

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
}
