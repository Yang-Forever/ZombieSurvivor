using UnityEngine;

public class ZombieSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    public Transform player;
    float spawnInterval = 3f;
    int spawnCountPerWave = 2;

    [Header("Spawn Area")]
    float spawnRadius = 20f;
    float minDistance = 15f;

    // 
    private int normalSpawnCounter = 0;

    private void Start()
    {
        if (player == null)
            player = GameObject.Find("Player").transform;

        InvokeRepeating(nameof(SpawnWave), 1f, spawnInterval);
    }

    void SpawnWave()
    {
        for (int i = 0; i < spawnCountPerWave; i++)
        {
            SpawnZombie();
        }
    }

    void SpawnZombie()
    {
        if (!player) return;

        Vector3 randomDir = Random.insideUnitSphere;
        randomDir.y = 0;

        float dist = Random.Range(minDistance, spawnRadius);
        Vector3 spawnPos = player.position + randomDir.normalized * dist;

        ZombieType zType;

        // 일반 4마리 스폰 후에 패스트 1마리 스폰
        if (normalSpawnCounter < 4)
        {
            zType = ZombieType.Normal;
            normalSpawnCounter++;
        }
        else
        {
            zType = ZombieType.Fast;
            normalSpawnCounter = 0;
        }

        ZombiePool.Inst.Spawn(zType, spawnPos);
    }
}
