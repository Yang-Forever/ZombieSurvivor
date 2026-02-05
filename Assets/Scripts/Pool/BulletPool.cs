using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 플레이어 총알 오브젝트 풀 관리
/// 총알 생성 / 재사용을 통해 성능 최적화 담당
/// </summary>
public class BulletPool : MonoBehaviour
{
    public GameObject playerBullet;
    public int poolCount = 150;         // 초기 풀 개수

    Queue<Bullet_Ctrl> pool = new Queue<Bullet_Ctrl>();

    public Transform playerBullets; // 총알 정리용 부모 오브젝트

    public static BulletPool Inst = null;

    private void Awake()
    {
        Inst = this;

        for (int i = 0; i < poolCount; i++)
        {
            CreateBullet();
        }
    }

    // 총알 1개 생성 후 풀에 등록
    void CreateBullet()
    {
        GameObject go = Instantiate(playerBullet, playerBullets);
        go.SetActive(false);

        Bullet_Ctrl bullet = go.GetComponent<Bullet_Ctrl>();
        bullet.SetPool(this);

        pool.Enqueue(bullet);
    }

    // 사용 가능한 총알 반환
    public Bullet_Ctrl Get()
    {
        if (pool.Count == 0)
            CreateBullet();

        Bullet_Ctrl bullet = pool.Dequeue();
        bullet.gameObject.SetActive(true);
        return bullet;
    }

    // 사용 완료된 총알을 다시 풀로 반환
    public void Return(Bullet_Ctrl bullet)
    {
        bullet.gameObject.SetActive(false);
        pool.Enqueue(bullet);
    }
}
