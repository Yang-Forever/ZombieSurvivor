using UnityEngine;

public class Bullet_Ctrl : MonoBehaviour
{
    private BulletPool pool;

    private float lifeTime;
    private float speed = 50.0f;
    private float damage;
    private int penetration;

    // Start is called before the first frame update
    private void OnEnable()
    {
        lifeTime = 3.0f;
    }

    // Update is called once per frame
    void Update()
    {
        transform.position += transform.forward * speed * Time.deltaTime;

        lifeTime -= Time.deltaTime;
        if(lifeTime <= 0)
            ReturnPool();
    }

    public void SetPool(BulletPool p)
    {
        pool = p;
    }

    void ReturnPool()
    {
        pool.Return(this);
    }

    public void SetDamage(float value)
    {
        damage = value;
    }

    public void SetPenetration(int value)
    {
        penetration = value + 1;
    }

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
