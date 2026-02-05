using UnityEngine;

/// <summary>
/// 화염방사기 무기의 발사, 히트 판정, 과열 시스템을 관리
/// </summary>
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

    // 화염 무기 초기화
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

    // 화염 발사 시작
    public void StartFire()
    {
        if (isOverHeat)
            return;


        isFiring = true;

        if (flameFX != null && !flameFX.isPlaying)
            flameFX.Play();

        Sound_Mgr.Inst.PlayLoopEffect("Flame", 0.6f);

        SetFlameActive(true);
    }

    // 화염 발사 중지
    public void StopFire()
    {
        isFiring = false;

        if (flameFX != null && flameFX.isPlaying)
            flameFX.Stop(true, ParticleSystemStopBehavior.StopEmitting);

        Sound_Mgr.Inst.StopLoopEffect("Flame");

        SetFlameActive(false);
    }

    // 히트 콜라이더 활성 / 비활성
    public void SetFlameActive(bool on)
    {
        if (hitCollider != null)
            hitCollider.enabled = on;
    }

    // 화염 위치, 길이, 과열 상태 갱신
    void Update()
    {
        if (data == null || firePos == null)
            return;

        flameRoot.transform.position = firePos.position;
        flameRoot.transform.rotation = firePos.rotation;

        ApplyLength();

        HandleHeat();
    }

    // 화염 지속 충돌 시 틱 데미지 처리
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

    // 화염 과열 및 냉각 처리
    void HandleHeat()
    {
        if (!isFiring)
        {
            // 식는 중
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

    // 화염 길이 및 파티클 속도 적용
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

    // 현재 과열 비율 반환
    public float GetHeatRatio()
    {
        return data == null ? 0f : heat / data.GetFlameMaxHeat();
    }

    // 과열 상태 여부 반환
    public bool IsOverHeat()
    {
        return isOverHeat;
    }
}
