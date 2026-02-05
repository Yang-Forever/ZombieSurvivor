using UnityEngine;
public enum SupplyType
{
    HpPotion,
    Orbital
}

/// <summary>
/// 총알에 맞으면 파괴
/// 확률에 따라 아이템 드랍
/// </summary>
public class SupplyBox_Ctrl : MonoBehaviour
{
    [Header("Drop Items")]
    public GameObject hpPotionPrefab;
    public GameObject orbitalPrefab;

    Animator animator;

    bool isDestroy = false; // 중복 충돌 방지

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    // 총알 충돌 시 아이템 드랍 처리
    private void OnTriggerEnter(Collider other)
    {
        if (isDestroy)
            return;

        if (other.CompareTag("Bullet"))
        {
            DropItem();
            Destroy(gameObject, 0.5f);
        }
    }

    // 확률에 따라 아이템 하나를 생성
    void DropItem()
    {
        isDestroy = true;

        GameObject prefab;

        int rand = Random.Range(1, 11);

        // 30% 스킬 , 70% HP 포션
        if (rand <= 3)
            prefab = orbitalPrefab;
        else
            prefab = hpPotionPrefab;

        Vector3 spawnPos = transform.position;
        spawnPos.y = 0.8f;

        Instantiate(prefab, spawnPos, Quaternion.identity);
    }
}
