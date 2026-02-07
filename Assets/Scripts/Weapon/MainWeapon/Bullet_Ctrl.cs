using UnityEngine;

/// <summary>
/// 총알의 이동, 수명 관리, 관통 처리
/// 오브젝트 풀 반환 담당
/// </summary>
public class Bullet_Ctrl : MonoBehaviour
{
    private BulletPool pool;

    float lifeTime;
    float speed = 50.0f;
    float damage;
    int penetration;

    // 오브젝트 풀에서 활성화될 때 수명 초기화
    private void OnEnable()
    {
        lifeTime = 3.0f;
    }

    // 총알 이동 및 수명 체크
    void Update()
    {
        transform.position += transform.forward * speed * Time.deltaTime;

        lifeTime -= Time.deltaTime;
        if(lifeTime <= 0)
            ReturnPool();
    }

    // 총알이 사용할 풀 설정
    public void SetPool(BulletPool p)
    {
        pool = p;
    }

    // 총알을 오브젝트 풀로 반환
    void ReturnPool()
    {
        pool.Return(this);
    }

    // 총알 데미지 설정
    public void SetDamage(float value)
    {
        damage = value;
    }

    // 관통 횟수 설정 (첫 타격 포함)
    public void SetPenetration(int value)
    {
        penetration = value + 1;
    }

    // 좀비 충돌 시 데미지 처리 및 관통 감소
    private void OnTriggerEnter(Collider coll)
    {
        if (coll.CompareTag("Zombie"))
        {
            Zombie_Ctrl zombie = coll.GetComponentInParent<Zombie_Ctrl>();
            if (zombie == null)
                return;

            zombie.HitDamage(damage);

            penetration--;

            if (penetration <= 0)
                ReturnPool();
        }
    }
}
