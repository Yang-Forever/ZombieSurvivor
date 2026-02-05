using UnityEngine;

/// <summary>
/// 지정된 위치로 낙하한 뒤
/// 폭발을 발생시키는 궤도 폭격 투사체
/// </summary>
public class OrbitalProjectile_Ctrl : MonoBehaviour
{
    public float fallSpeed = 40f;
    public GameObject explosionPrefab;

    Vector3 targetPos;

    // 폭격 목표 위치 설정
    public void SetTarget(Vector3 pos)
    {
        targetPos = pos;
    }

    // 목표 지점으로 낙하 이동 처리
    void Update()
    {
        transform.position = Vector3.MoveTowards(transform.position, targetPos, fallSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, targetPos) < 0.5f)
        {
            Explosion();
        }
    }

    // 폭발 이펙트 및 사운드 처리
    void Explosion()
    {
        Sound_Mgr.Inst.PlayEffSoundLimit("OrbitalCall", 0.8f, 0.3f);
        Instantiate(explosionPrefab, targetPos, Quaternion.identity);
        Destroy(gameObject);
    }
}
