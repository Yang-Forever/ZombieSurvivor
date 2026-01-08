using UnityEngine;

public class Bullet_Ctrl : MonoBehaviour
{
    private float speed = 30.0f;
    private float damage;
    private int penetration;

    // Start is called before the first frame update
    void Start()
    {
        Destroy(gameObject, 3.0f);
    }

    // Update is called once per frame
    void Update()
    {
        transform.position += transform.forward * speed * Time.deltaTime;
    }

    public void SetDamage(float value)
    {
        damage = value;
    }

    public void SetPenetration(int value)
    {
        penetration = value;
    }

    private void OnTriggerEnter(Collider coll)
    {
        if (coll.CompareTag("Zombie"))
        {
            Debug.Log("Hit");
            coll.gameObject.GetComponent<Zombie_Ctrl>().HitDamage(damage);

            penetration--;

            if (penetration < 0)
                Destroy(gameObject);
        }
    }
}
