using UnityEngine;

public class ZombieAnimEvent : MonoBehaviour
{
    Zombie_Ctrl zombie;

    // Start is called before the first frame update
    void Start()
    {
        zombie = GetComponentInParent<Zombie_Ctrl>();
    }

    public void Event_AtkHit()
    {
        if (zombie != null)
            zombie.OnAtkHit();
    }

    public void Event_AttackEnd()
    {
        if (zombie != null)
            zombie.OnAttackEnd();
    }
}
