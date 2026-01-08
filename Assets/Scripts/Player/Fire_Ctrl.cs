using UnityEngine;

public class Fire_Ctrl : MonoBehaviour
{
    GameObject bulletPrefab = null;
    public GameObject firePos = null;

    ItemRuntimeData data;

    private float fireTimer;

    // Start is called before the first frame update
    void Start()
    {
        bulletPrefab = (GameObject)Resources.Load("Bullet");

        if (firePos == null)
            firePos = GameObject.Find("FirePos");

        fireTimer = 0.0f;
    }

    // Update is called once per frame
    void Update()
    {
        if(GameMgr.Inst.state != PlayerState.Play)
            return;

        data = Gun.Inst.curWeapon;

        fireTimer -= Time.deltaTime;
        Fire(data);
    }

    // 수동
    void Fire(ItemRuntimeData runtimeData)
    {
        if (fireTimer > 0.0f)
            return;

        if (Input.GetMouseButton(0) && !GameMgr.IsPointerOverUIObject())
        {
            if(data.baseData.itemName == "Shotgun")
            {
                SpreadFire();
            }
            else
            {
                StraightFire();
            }

            fireTimer = runtimeData.GetInterval();
        }
    }

    void StraightFire()
    {
        GameObject go = Instantiate(bulletPrefab, firePos.transform.position, firePos.transform.rotation);
        Bullet_Ctrl bullet = go.GetComponent<Bullet_Ctrl>();

        float damage = data.baseData.baseDamage * data.GetDamageRatio() * PlayerStats.Inst.DamageMultiplier;    // 기본 무기 데미지 * 무기 데미지 증감률 * 스텟 공격력 증감률
        int penetration = data.GetPenetration() + PlayerStats.Inst.Penetration;

        bullet.SetDamage(damage);
        bullet.SetPenetration(penetration);
    }

    void SpreadFire()
    {
        int pelletCount = data.GetPelletCount();

        float maxAngle = 15.0f;

        float damage = data.baseData.baseDamage * data.GetDamageRatio() * PlayerStats.Inst.DamageMultiplier;
        int penetration = data.GetPenetration() + PlayerStats.Inst.Penetration;

        float step = (maxAngle * 2f) / (pelletCount - 1);
        float startAngle = -maxAngle;

        for (int i = 0; i < pelletCount; i++)
        {
            float angle = startAngle + step * i;

            GameObject go = Instantiate(bulletPrefab, firePos.transform.position, firePos.transform.rotation);
            Bullet_Ctrl bullet = go.GetComponent<Bullet_Ctrl>();

            go.transform.Rotate(0, angle, 0);

            bullet.SetDamage(damage);
            bullet.SetPenetration(penetration);
        }
    }

}
