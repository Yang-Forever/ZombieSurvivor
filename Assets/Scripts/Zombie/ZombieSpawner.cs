using UnityEngine;

/// <summary>
/// 플레이어 주변에서 좀비를 스폰하고 난이도 및 보스 출현을 관리
/// </summary>
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
    float safeMargin = 1.5f;

    [Header("Map Bounds")]
    public BoxCollider mapBounds;     // 맵 전체 영역

    [Header("Difficulty")]
    public int difficultyLevel = 0;

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

    // 스폰 타이머 갱신 및 좀비 생성 처리
    void Update()
    {
        if (GameMgr.Inst.state != GameState.Play)
            return;

        spawnTimer += Time.deltaTime;
        float interval = 1f / spawnPerSecond;

        if (spawnTimer >= interval)
        {
            spawnTimer = 0f;
            SpawnZombie();
        }
    }

    // 폭탄 좀비 등장 카운터 초기화
    void ResetBombCounter()
    {
        spawnCount = 0;
        nextBombAt = Random.Range(10, 16); // 10~15
    }

    // 일반 좀비 스폰 처리
    void SpawnZombie()
    {
        if (!player || !mapBounds)
            return;

        if (!TryGetSpawnPosAroundPlayer(out Vector3 spawnPos))
            return;

        ZombiePool.Inst.Spawn(DecideZombieType(), spawnPos);
    }

    // 스폰될 좀비 타입 결정
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

    // 난이도 증가 처리
    public void IncreaseDifficulty(int level)
    {
        difficultyLevel = level;

        Zombie_Ctrl.NormalHpMul = 1f + level * 0.1f;
        Zombie_Ctrl.NormalSpeedMul = 1f + level * 0.05f;
        Zombie_Ctrl.NormalDmgMul = 1f + level * 0.1f;

        spawnPerSecond = 1.5f + level * 0.5f;
    }

    // 보스 좀비 스폰 처리
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

    // 보스 난이도 증가 처리
    public void IncreaseBossDifficulty(int level)
    {
        Zombie_Ctrl.BossHpMul = 1f + level * 0.5f;
        Zombie_Ctrl.BossSpeedMul = 1f + level * 0.05f;
        Zombie_Ctrl.BossDmgMul = 1f + level * 0.1f;
    }

    // 플레이어 주변 유효 스폰 위치 탐색
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

            if (pos.x < b.min.x + safeMargin || pos.x > b.max.x - safeMargin ||
                pos.z < b.min.z + safeMargin || pos.z > b.max.z - safeMargin)
                continue;

            result = pos;
            return true;
        }

        result = Vector3.zero;
        return false;
    }

    // 스포너 상태 초기화
    public void ResetSpawner()
    {
        difficultyLevel = 0;
        spawnPerSecond = 1.5f;
    }

}
