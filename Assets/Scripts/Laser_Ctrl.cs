using UnityEngine;

public class Laser_Ctrl : MonoBehaviour
{
    [Header("Reference")]
    [SerializeField] MeshRenderer visual;
    [SerializeField] Collider hitCollider;

    Transform firePos;
    ItemRuntimeData data;

    [Header("Laser State")]
    bool isFiring = false;
    bool isOverHeat = false;
    [SerializeField] Transform laserRoot;

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

        SetLaserActive(false);
        ApplyLength();
    }

    public void StartFire()
    {
        if (isOverHeat)
            return;

        isFiring = true;
        SetLaserActive(true);
    }

    public void StopFire()
    {
        isFiring = false;
        SetLaserActive(false);
    }

    public void SetLaserActive(bool on)
    {
        if (visual != null)
            visual.enabled = on;

        if (hitCollider != null)
            hitCollider.enabled = on;
    }

    void Update()
    {
        if (data == null || firePos == null)
            return;

        laserRoot.transform.position = firePos.position;
        laserRoot.transform.rotation = firePos.rotation;

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
            float damage = data.GetLaserTickDamage();
            z.HitDamage(damage);
        }

        hitTimer = data.GetLaserTickInterval();
    }

    void HandleHeat()
    {
        if (!isFiring)
        {
            // ½Ä´Â Áß
            heat -= data.GetLaserCoolDown() * Time.deltaTime;
            if (heat <= 0f)
            {
                heat = 0f;
                isOverHeat = false;
            }
            return;
        }

        if (isOverHeat)
            return;

        heat += data.GetLaserHeatIncrease() * Time.deltaTime;

        if (heat >= data.GetLaserMaxHeat())
        {
            heat = data.GetLaserMaxHeat();
            isOverHeat = true;
            StopFire();
        }
    }
    public void ApplyLength()
    {
        if (data == null || laserRoot == null)
            return;

        Vector3 scale = laserRoot.localScale;
        scale.z = data.GetLength();

        laserRoot.localScale = scale;
    }

    public float GetHeatRatio()
    {
        return data == null ? 0f : heat / data.GetLaserMaxHeat();
    }

    public bool IsOverHeat()
    {
        return isOverHeat;
    }
}
