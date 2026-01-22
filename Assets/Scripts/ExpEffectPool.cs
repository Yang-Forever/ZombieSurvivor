using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExpEffectPool : MonoBehaviour
{
    public GameObject effectPrefab;
    public int poolSize = 30;

    Queue<GameObject> pool = new Queue<GameObject>();
    public Transform expEffects;

    public static ExpEffectPool Inst;

    void Awake()
    {
        Inst = this;

        for (int i = 0; i < poolSize; i++)
        {
            GameObject obj = Instantiate(effectPrefab, expEffects);
            obj.SetActive(false);
            pool.Enqueue(obj);
        }
    }

    public void PlayEffect(Vector3 pos)
    {
        GameObject obj = pool.Dequeue();
        obj.transform.position = pos;
        obj.SetActive(true);

        StartCoroutine(ReturnRoutine(obj, 0.5f));

        pool.Enqueue(obj);
    }

    IEnumerator ReturnRoutine(GameObject obj, float time)
    {
        yield return new WaitForSeconds(time);
        obj.SetActive(false);
    }
}
