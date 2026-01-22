using System.Collections.Generic;
using UnityEngine;

public class BulletPool : MonoBehaviour
{
    public GameObject playerBullet;

    public int poolCount = 150;

    Queue<Bullet_Ctrl> pool = new Queue<Bullet_Ctrl>();

    public Transform playerBullets;

    public static BulletPool Inst = null;

    private void Awake()
    {
        Inst = this;

        for (int i = 0; i < poolCount; i++)
        {
            CreateBullet();
        }
    }

    void CreateBullet()
    {
        GameObject go = Instantiate(playerBullet, playerBullets);
        go.SetActive(false);

        Bullet_Ctrl bullet = go.GetComponent<Bullet_Ctrl>();
        bullet.SetPool(this);

        pool.Enqueue(bullet);
    }

    public Bullet_Ctrl Get()
    {
        if (pool.Count == 0)
            CreateBullet();

        Bullet_Ctrl bullet = pool.Dequeue();
        bullet.gameObject.SetActive(true);
        return bullet;
    }

    public void Return(Bullet_Ctrl bullet)
    {
        bullet.gameObject.SetActive(false);
        pool.Enqueue(bullet);
    }
}
