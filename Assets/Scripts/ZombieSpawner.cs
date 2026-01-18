using UnityEngine;

public class ZombieSpawner : MonoBehaviour
{
    [Header("Target")]
    public Transform player;

    [Header("Spawn Rate")]
    public float spawnPerSecond = 1.5f;
    float spawnTimer;

    [Header("Spawn Distance")]
    public float minDistance = 12f;   // 원 안쪽 금지
    public float maxDistance = 17f;   // 원 안쪽 금지

    [Header("Map Bounds")]
    public BoxCollider mapBounds;     // 맵 전체 영역

    [Header("Difficulty")]
    public int difficultyLevel = 0;
    int normalCounter = 0;

    [Header("BombZombie Rate")]
    int spawnCount = 0;
    int nextBombAt = 0;

    public static ZombieSpawner Inst;

    private void Awake()
    {
        Inst = this;

        if (player == null)
            player = GameObject.Find("Player").transform;

        ResetBombCounter();
    }

    void Update()
    {
        if (GameMgr.Inst.state != PlayerState.Play)
            return;

        spawnTimer += Time.deltaTime;
        float interval = 1f / spawnPerSecond;

        if (spawnTimer >= interval)
        {
            spawnTimer = 0f;
            SpawnZombie();
        }
    }
    void ResetBombCounter()
    {
        spawnCount = 0;
        nextBombAt = Random.Range(10, 16); // 10~15
    }

    void SpawnZombie()
    {
        if (!player || !mapBounds)
            return;

        if (!TryGetSpawnPosAroundPlayer(out Vector3 spawnPos))
            return;

        ZombiePool.Inst.Spawn(DecideZombieType(), spawnPos);
    }

    ZombieType DecideZombieType()
    {
        spawnCount++;

        if (spawnCount >= nextBombAt)
        {
            ResetBombCounter();
            return ZombieType.Explosion;
        }

        return ZombieType.Normal;
    }

    public void IncreaseDifficulty(int level)
    {
        difficultyLevel = level;

        Zombie_Ctrl.NormalHpMul = 1f + level * 0.1f;
        Zombie_Ctrl.NormalSpeedMul = 1f + level * 0.05f;
        Zombie_Ctrl.NormalDmgMul = 1f + level * 0.1f;

        spawnPerSecond = 1.5f + level * 0.5f;
    }

    public void SpawnBoss(int bossLevel)
    {
        if (!player || !mapBounds)
            return;

        IncreaseBossDifficulty(bossLevel);

        float prevMin = minDistance;
        float prevMax = maxDistance;

        minDistance = 15f;
        maxDistance = 22f;

        if (!TryGetSpawnPosAroundPlayer(out Vector3 spawnPos))
        {
            minDistance = prevMin;
            maxDistance = prevMax;
            return;
        }

        minDistance = prevMin;
        maxDistance = prevMax;

        ZombiePool.Inst.SpawnBoss(spawnPos);
    }


    public void IncreaseBossDifficulty(int level)
    {
        Zombie_Ctrl.BossHpMul = 1f + level * 0.5f;
        Zombie_Ctrl.BossSpeedMul = 1f + level * 0.05f;
        Zombie_Ctrl.BossDmgMul = 1f + level * 0.1f;
    }
    bool TryGetSpawnPosAroundPlayer(out Vector3 result)
    {
        const int maxTry = 30;
        Bounds b = mapBounds.bounds;

        for (int i = 0; i < maxTry; i++)
        {
            float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            Vector3 dir = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle));

            float dist = Random.Range(minDistance, maxDistance);
            Vector3 pos = player.position + dir * dist;
            pos.y = 0f;

            if (pos.x < b.min.x || pos.x > b.max.x ||
                pos.z < b.min.z || pos.z > b.max.z)
                continue;

            result = pos;
            return true;
        }

        result = Vector3.zero;
        return false;
    }


    public void ResetSpawner()
    {
        difficultyLevel = 0;
        spawnPerSecond = 1.5f;
    }

}
