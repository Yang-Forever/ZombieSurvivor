using UnityEngine;

public class Fire_Ctrl : MonoBehaviour
{
    public Transform firePos = null;

    ItemRuntimeData data;

    private float fireTimer;

    [Header("Laser")]
    public Laser_Ctrl laser;
    [SerializeField] LaserUI laserUI;
    bool laserInited = false;

    // Start is called before the first frame update
    void Start()
    {
        if (firePos == null)
            firePos = GameObject.Find("FirePos").transform;

        fireTimer = 0.0f;

        if (laser != null)
            laser.SetLaserActive(false);

        if (laserUI != null)
            laserUI.SetVisible(false);

        laserInited = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (GameMgr.Inst.state != PlayerState.Play)
            return;

        data = Gun.Inst.curWeapon;
        if (data == null)
            return;

        bool isLaserWeapon = data.baseData.mainWeapon == MainWeaponType.Lazer;
        laserUI.SetVisible(isLaserWeapon);

        if (isLaserWeapon)
        {
            if (!laserInited)
            {
                laser.Init(firePos, data);
                laserInited = true;
            }

            LaserUpdate();
        }
        else
        {
            if (laserInited)
            {
                laser.StopFire();
                laserInited = false;
            }

            fireTimer -= Time.deltaTime;
            Fire(data);
        }
    }

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

            fireTimer = runtimeData.GetInterval();
        }
    }

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

    void SpreadFire()
    {
        int pelletCount = data.GetPelletCount();

        float maxAngle = 15.0f;

        float damage = data.baseData.baseDamage * data.GetDamageRatio() * PlayerStats.Inst.DamageMultiplier;
        int penetration = data.baseData.penetration + PlayerStats.Inst.Penetration;

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

    void LaserUpdate()
    {
        if (laser == null)
            return;

        bool isPressing = Input.GetMouseButton(0) && !GameMgr.IsPointerOverUIObject();

        if (isPressing)
            laser.StartFire();
        else if(!isPressing)
            laser.StopFire();
    }
}
