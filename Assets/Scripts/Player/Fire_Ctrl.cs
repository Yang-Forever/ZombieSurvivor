using UnityEngine;

/// <summary>
/// 플레이어 무기 발사 전반을 제어
/// 일반 탄환 무기와 화염방사기(레이저 계열)를 구분하여 처리한다
/// </summary>
public class Fire_Ctrl : MonoBehaviour
{
    public Transform firePos = null;

    ItemRuntimeData data;

    float fireTimer;

    [Header("Flame")]
    public Flame_Ctrl flame;
    [SerializeField] FlameUI flameUI;
    bool flameInited = false;

    // Start is called before the first frame update
    void Start()
    {
        if (firePos == null)
            firePos = GameObject.Find("FirePos").transform;

        fireTimer = 0.0f;

        if (flame != null)
            flame.SetFlameActive(false);

        if (flameUI != null)
            flameUI.SetVisible(false);

        flameInited = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (GameMgr.Inst.state != GameState.Play)
            return;

        data = Gun.Inst.curWeapon;
        if (data == null)
            return;

        // 화염방사기 여부 판단
        bool isFlameWeapon = data.baseData.mainWeapon == MainWeaponType.Flamethrower;
        flameUI.SetVisible(isFlameWeapon);

        if (isFlameWeapon)
        {
            // 화염 무기 최초 진입 시 초기화
            if (!flameInited)
            {
                flame.Init(firePos, data);
                flameInited = true;
            }

            LaserUpdate();
        }
        else
        {
            // 다른 무기로 전환 시 화염 상태 정리
            if (flameInited)
            {
                flame.StopFire();
                flameInited = false;
            }

            fireTimer -= Time.deltaTime;
            Fire(data);
        }
    }

    // 일반 무기 발사 처리
    void Fire(ItemRuntimeData runtimeData)
    {
        if (fireTimer > 0.0f)
            return;

        if (Input.GetMouseButton(0) && !GameMgr.IsPointerOverUIObject())
        {
            Sound_Mgr.Inst.PlayEffSoundLimit("Shot", 0.6f, 0.08f);

            if (data.baseData.mainWeapon == MainWeaponType.Shotgun)
                SpreadFire();
            else
                StraightFire();

            // 무기 공격 간격 적용
            fireTimer = runtimeData.GetInterval();
        }
    }

    // 직선 발사 무기 처리
    void StraightFire()
    {
        Bullet_Ctrl bullet = BulletPool.Inst.Get();

        bullet.transform.position = firePos.position;
        bullet.transform.rotation = firePos.rotation;

        float damage = data.baseData.baseDamage * data.GetDamageRatio() * PlayerStats.Inst.DamageMultiplier;    // 기본 무기 데미지 * 무기 데미지 증감률 * 스텟 공격력 증감률
        int penetration = data.baseData.penetration + PlayerStats.Inst.Penetration;

        bullet.SetDamage(damage);
        bullet.SetPenetration(penetration);
    }

    // 샷건 계열 확산 발사 처리
    void SpreadFire()
    {
        int pelletCount = data.GetPelletCount();

        float maxAngle = 15.0f;

        float damage = data.baseData.baseDamage * data.GetDamageRatio() * PlayerStats.Inst.DamageMultiplier;
        int penetration = data.baseData.penetration + PlayerStats.Inst.Penetration;

        // 전체 각도를 pellet 개수로 분할
        float step = (maxAngle * 2f) / (pelletCount - 1);
        float startAngle = -maxAngle;

        for (int i = 0; i < pelletCount; i++)
        {
            float angle = startAngle + step * i;

            Bullet_Ctrl bullet = BulletPool.Inst.Get();

            bullet.transform.position = firePos.position;
            bullet.transform.rotation = firePos.rotation;
            bullet.transform.Rotate(0f, angle, 0f);

            bullet.SetDamage(damage);
            bullet.SetPenetration(penetration);
        }
    }

    // 화염방사기 입력 및 사운드 처리
    void LaserUpdate()
    {
        if (flame == null)
            return;

        bool isPressing = Input.GetMouseButton(0) && !GameMgr.IsPointerOverUIObject();

        if (isPressing)
        {
            flame.StartFire();
        }
        else if(!isPressing)
        {
            flame.StopFire();
        }
    }
}
