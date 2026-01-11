using UnityEngine;
using UnityEngine.AI;

public class ZombieSpawner : MonoBehaviour
{
    [Header("Target")]
    public Transform player;

    [Header("Spawn Rate")]
    public float spawnPerSecond = 1.5f;
    float spawnTimer;

    [Header("Spawn Area")]
    float spawnRadius = 20f;
    float minDistance = 15f;

    [Header("Difficulty")]
    public int difficultyLevel = 0;
    int normalCounter = 0;

    public static ZombieSpawner Inst = null;

    private void Awake()
    {
        Inst = this;

        if (player == null)
            player = GameObject.Find("Player").transform;
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

    #region 스폰 로직
    void SpawnZombie()
    {
        if (!player)
            return;

        int maxTry = 10;

        for (int i = 0; i < maxTry; i++)
        {
            Vector3 dir = Random.insideUnitSphere;
            dir.y = 0f;
            dir.Normalize();

            float dist = Random.Range(minDistance, spawnRadius);
            Vector3 spawnPos = player.position + dir * dist;

            if (NavMesh.SamplePosition(spawnPos, out NavMeshHit hit, 2f, NavMesh.AllAreas))
            {
                ZombieType type = DecideZombieType();
                ZombiePool.Inst.Spawn(type, hit.position);
                return;
            }
        }
    }

    ZombieType DecideZombieType()
    {
        // 초반엔 Normal 위주 → 점점 Fast 증가
        int fastRate = Mathf.Clamp(difficultyLevel, 1, 5);

        if (normalCounter < fastRate)
        {
            normalCounter++;
            return ZombieType.Normal;
        }
        else
        {
            normalCounter = 0;
            return ZombieType.Fast;
        }
    }

    public void IncreaseDifficulty(int level)
    {
        difficultyLevel = level;

        // 전역 스텟 배율
        Zombie_Ctrl.HpMultiplier = 1f + level * 0.1f;
        Zombie_Ctrl.SpeedMultiplier = 1f + level * 0.05f;
        Zombie_Ctrl.DamageMultiplier = 1f + level * 0.1f;

        // 스폰량 증가
        spawnPerSecond = 1.5f + level * 0.5f;
    }

    public void SpawnBoss()
    {
        if (!player)
            return;

        Vector3 dir = Random.insideUnitSphere;
        dir.y = 0f;
        dir.Normalize();

        Vector3 pos = player.position + dir * 18f;

        if (NavMesh.SamplePosition(pos, out NavMeshHit hit, 2f, NavMesh.AllAreas))
        {
            ZombiePool.Inst.SpawnBoss(hit.position);
        }
    }
    #endregion
}
