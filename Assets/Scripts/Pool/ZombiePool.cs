using UnityEngine;
using UnityEngine.Pool;

/// <summary>
/// 일반 / 폭탄 좀비는 ObjectPool 사용
/// 보스 좀비는 단일 인스턴스 생성 방식
/// </summary>
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

        // 일반 좀비 풀
        normalPool = new ObjectPool<Zombie_Ctrl>(CreateNormalZombie, OnGetZombie, OnReleaseZombie, OnDestroyZombie, true, 30, 120);

        // 폭탄 좀비 풀
        bombPool = new ObjectPool<Zombie_Ctrl>(CreateFastZombie, OnGetZombie, OnReleaseZombie, OnDestroyZombie, true, 20, 80);
    }

    #region CreateZombie
    // 일반 좀비 생성
    Zombie_Ctrl CreateNormalZombie()
    {
        Zombie_Ctrl z = Instantiate(normalZombie).GetComponent<Zombie_Ctrl>();
        z.transform.SetParent(zombiesSpawner);
        z.SetPool(this);
        z.gameObject.SetActive(false);
        return z;
    }

    // 폭탄 좀비 생성
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
    // 풀에서 꺼낼 때 호출 (활성화는 Spawn에서 직접 처리)
    void OnGetZombie(Zombie_Ctrl z)
    {
        //z.gameObject.SetActive(true);
    }

    // 풀로 반환될 때 호출
    void OnReleaseZombie(Zombie_Ctrl z)
    {
        z.gameObject.SetActive(false);
    }

    // 풀 최대치 초과 시 완전 제거
    void OnDestroyZombie(Zombie_Ctrl z)
    {
        Destroy(z.gameObject);
    }

    // 좀비 타입에 따라 알맞은 풀로 반환
    public void ReturnZombie(Zombie_Ctrl z)
    {
        if(z.zomType == ZombieType.Normal)
            normalPool.Release(z);
        else if(z.zomType == ZombieType.Explosion)
            bombPool.Release(z);
    }
    #endregion

    // 일반 / 폭탄 좀비 스폰
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

        // ObjectPool Get 이후 공통 초기화
        z.gameObject.SetActive(false);
        z.zomType = type;
        z.transform.SetParent(zombiesSpawner);
        z.transform.position = pos;
        z.ResetZombie();
        z.gameObject.SetActive(true);

        return z;
    }

    // 보스 좀비 스폰 (풀링 미사용)
    public Zombie_Ctrl SpawnBoss(Vector3 pos)
    {
        Zombie_Ctrl boss = Instantiate(bossZombie, pos, Quaternion.identity).GetComponent<Zombie_Ctrl>();

        boss.zomType = ZombieType.Boss;
        boss.ResetZombie();

        return boss;
    }
}
