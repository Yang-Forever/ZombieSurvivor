using UnityEngine;

public class Flame_Ctrl : MonoBehaviour
{
    [Header("Reference")]
    [SerializeField] Collider hitCollider;
    [SerializeField] ParticleSystem flameFX;

    Transform firePos;
    ItemRuntimeData data;

    [Header("Flame State")]
    bool isFiring = false;
    bool isOverHeat = false;
    [SerializeField] Transform flameRoot;

    float heat = 0f;
    float hitTimer = 0f;

    public void Init(Transform firePos, ItemRuntimeData runtimeData)
    {
        this.firePos = firePos;
        this.data = runtimeData;

        heat = 0f;
        hitTimer = 0f;
        isOverHeat = false;
        isFiring = false;

        SetFlameActive(false);
        ApplyLength();
    }

    public void StartFire()
    {
        if (isOverHeat)
            return;

        isFiring = true;

        if (flameFX != null && !flameFX.isPlaying)
            flameFX.Play();

        SetFlameActive(true);
    }

    public void StopFire()
    {
        isFiring = false;

        if (flameFX != null && flameFX.isPlaying)
            flameFX.Stop(true, ParticleSystemStopBehavior.StopEmitting);

        SetFlameActive(false);
    }

    public void SetFlameActive(bool on)
    {
        if (hitCollider != null)
            hitCollider.enabled = on;
    }

    void Update()
    {
        if (data == null || firePos == null)
            return;

        flameRoot.transform.position = firePos.position;
        flameRoot.transform.rotation = firePos.rotation;

        ApplyLength();

        HandleHeat();
    }

    void OnTriggerStay(Collider other)
    {
        if (!isFiring || isOverHeat)
            return;

        if (!other.CompareTag("Zombie"))
            return;

        hitTimer -= Time.deltaTime;
        if (hitTimer > 0f)
            return;

        Zombie_Ctrl z = other.GetComponentInParent<Zombie_Ctrl>();
        if (z != null && !z.isDead)
        {
            float damage = data.GetFlameTickDamage();
            z.HitDamage(damage);
        }

        hitTimer = data.GetFlameTickInterval();
    }

    void HandleHeat()
    {
        if (!isFiring)
        {
            // ½Ä´Â Áß
            heat -= data.GetFlameCoolDown() * Time.deltaTime;
            if (heat <= 0f)
            {
                heat = 0f;
                isOverHeat = false;
            }
            return;
        }

        if (isOverHeat)
            return;

        heat += data.GetFlameHeatIncrease() * Time.deltaTime;

        if (heat >= data.GetFlameMaxHeat())
        {
            heat = data.GetFlameMaxHeat();
            isOverHeat = true;
            StopFire();
        }
    }

    public void ApplyLength()
    {
        if (data == null || flameRoot == null)
            return;

        Vector3 scale = flameRoot.localScale;
        scale.z = data.GetLength();
        flameRoot.localScale = scale;

        var main = flameFX.main;
        main.startSpeed = data.GetLength();
    }

    public float GetHeatRatio()
    {
        return data == null ? 0f : heat / data.GetFlameMaxHeat();
    }

    public bool IsOverHeat()
    {
        return isOverHeat;
    }
}
