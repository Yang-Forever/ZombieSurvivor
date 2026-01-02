using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class ZombiePool : MonoBehaviour
{
    [Header("Zombie Prefabs")]
    public GameObject normalZombie;
    public GameObject fastZombie;

    public ObjectPool<Zombie_Ctrl> normalPool;
    public ObjectPool<Zombie_Ctrl> fastPool;

    public Transform zombiesSpawner;

    public static ZombiePool Inst = null;

    private void Awake()
    {
        Inst = this;

        normalPool = new ObjectPool<Zombie_Ctrl>(CreateNormalZombie, OnGetZombie, OnReleaseZombie, OnDestroyZombie, true, 30, 120);
        fastPool = new ObjectPool<Zombie_Ctrl>(CreateFastZombie, OnGetZombie, OnReleaseZombie, OnDestroyZombie, true, 20, 80);
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
        Zombie_Ctrl z = Instantiate(fastZombie).GetComponent<Zombie_Ctrl>();
        z.transform.SetParent(zombiesSpawner);
        z.SetPool(this);
        z.gameObject.SetActive(false);
        return z;
    }
    #endregion

    #region Get, Release, Destroy, Return Zombie
    void OnGetZombie(Zombie_Ctrl z)
    {
        z.gameObject.SetActive(true);
        z.ResetZombie();
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
        else if(z.zomType == ZombieType.Fast)
            fastPool.Release(z);
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

            case ZombieType.Fast:
                z = fastPool.Get();
                break;
        }
        z.transform.SetParent(zombiesSpawner);

        z.transform.position = pos;
        z.zomType = type;

        return z;
    }

}
