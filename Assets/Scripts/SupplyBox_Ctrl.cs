using UnityEngine;
public enum SupplyType
{
    HpPotion,
    Orbital
}

public class SupplyBox_Ctrl : MonoBehaviour
{
    [Header("Drop Items")]
    public GameObject hpPotionPrefab;
    public GameObject orbitalPrefab;

    Animator animator;

    bool isDestroy = false;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isDestroy)
            return;

        if (other.CompareTag("Bullet"))
        {
            animator.SetBool("Destroy", true);
            DropItem();
            Destroy(gameObject, 2.0f);
        }
    }

    void DropItem()
    {
        isDestroy = true;

        GameObject prefab;

        int rand = Random.Range(1, 11);

        if (rand <= 3)
            prefab = orbitalPrefab;
        else
            prefab = hpPotionPrefab;

        Vector3 spawnPos = transform.position;
        spawnPos.y = 0.8f;

        Instantiate(prefab, spawnPos, Quaternion.identity);
    }
}
