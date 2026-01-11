using UnityEngine;

public class DashHitBox : MonoBehaviour
{
    Zombie_Ctrl boss;
    bool isHit = false;

    // Start is called before the first frame update
    void Start()
    {
        boss = GetComponentInParent<Zombie_Ctrl>();
        gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        isHit = false;
    }

    void OnTriggerEnter(Collider coll)
    {
        if (coll.CompareTag("Wall"))
        {
            boss.EndDash();
            return;
        }

        if (isHit)
            return;

        if (coll.CompareTag("Player"))
        {
            Player_Ctrl player = coll.GetComponent<Player_Ctrl>();
            if (player != null)
            {
                player.HitDamage(40);
                isHit = true;
            }
        }
    }
}
