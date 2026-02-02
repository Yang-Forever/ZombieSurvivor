using UnityEngine;

public class OrbitalProjectile_Ctrl : MonoBehaviour
{
    public float fallSpeed = 40f;
    public GameObject explosionPrefab;

    Vector3 targetPos;

    public void SetTarget(Vector3 pos)
    {
        targetPos = pos;
    }

    void Update()
    {
        transform.position = Vector3.MoveTowards(transform.position, targetPos, fallSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, targetPos) < 0.5f)
        {
            Explosion();
        }
    }

    void Explosion()
    {
        Sound_Mgr.Inst.PlayEffSoundLimit("OrbitalCall", 0.8f, 0.3f);
        Instantiate(explosionPrefab, targetPos, Quaternion.identity);
        Destroy(gameObject);
    }
}
