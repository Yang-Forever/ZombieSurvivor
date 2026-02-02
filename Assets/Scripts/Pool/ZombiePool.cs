using UnityEngine;
using UnityEngine.Pool;

public class ZombiePool : MonoBehaviour
{
    [Header("Zombie Prefabs")]
    public GameObject normalZombie;
    public GameObject bombZombie;
    public GameObject bossZombie;

    public ObjectPool<Zombie_Ctrl> normalPool;
    public ObjectPool<Zombie_Ctrl> bombPool;

    public Transform zombiesSpawner;

    public static ZombiePool Inst = null;

    private void Awake()
    {
        Inst = this;

        normalPool = new ObjectPool<Zombie_Ctrl>(CreateNormalZombie, OnGetZombie, OnReleaseZombie, OnDestroyZombie, true, 30, 120);
        bombPool = new ObjectPool<Zombie_Ctrl>(CreateFastZombie, OnGetZombie, OnReleaseZombie, OnDestroyZombie, true, 20, 80);
    }

    #region CreateZombie
    Zombie_Ctrl CreateNormalZombie()
    {
        Zombie_Ctrl z = Instantiate(normalZombie).GetComponent<Zombie_Ctrl>();
        z.transform.SetParent(zombiesSpawner);
        z.SetPool(this);
        z.gameObject.SetActive(false);
        return z;
    }

    Zombie_Ctrl CreateFastZombie()
    {
        Zombie_Ctrl z = Instantiate(bombZombie).GetComponent<Zombie_Ctrl>();
        z.transform.SetParent(zombiesSpawner);
        z.SetPool(this);
        z.gameObject.SetActive(false);
        return z;
    }
    #endregion

    #region Get, Release, Destroy, Return Zombie
    void OnGetZombie(Zombie_Ctrl z)
    {
        //z.gameObject.SetActive(true);
    }

    void OnReleaseZombie(Zombie_Ctrl z)
    {
        z.gameObject.SetActive(false);
    }

    void OnDestroyZombie(Zombie_Ctrl z)
    {
        Destroy(z.gameObject);
    }

    public void ReturnZombie(Zombie_Ctrl z)
    {
        if(z.zomType == ZombieType.Normal)
            normalPool.Release(z);
        else if(z.zomType == ZombieType.Explosion)
            bombPool.Release(z);
    }
    #endregion
    public Zombie_Ctrl Spawn(ZombieType type, Vector3 pos)
    {
        Zombie_Ctrl z = null;

        switch (type)
        {
            case ZombieType.Normal:
                z = normalPool.Get();
                break;
            case ZombieType.Explosion:
                z = bombPool.Get();
                break;
        }

        z.gameObject.SetActive(false);
        z.zomType = type;
        z.transform.SetParent(zombiesSpawner);
        z.transform.position = pos;
        z.ResetZombie();
        z.gameObject.SetActive(true);

        return z;
    }

    public Zombie_Ctrl SpawnBoss(Vector3 pos)
    {
        Zombie_Ctrl boss = Instantiate(bossZombie, pos, Quaternion.identity).GetComponent<Zombie_Ctrl>();

        boss.zomType = ZombieType.Boss;
        boss.ResetZombie();

        return boss;
    }
}
