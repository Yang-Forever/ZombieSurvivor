using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 경험치 휙득 이펙트 오브젝트 풀
/// 이펙트 재사용을 통해 Instantiate 비용 최소화
/// </summary>
public class ExpEffectPool : MonoBehaviour
{
    public GameObject effectPrefab;
    public int poolSize = 30;       // 초기 풀 크기

    Queue<GameObject> pool = new Queue<GameObject>();
    public Transform expEffects;

    public static ExpEffectPool Inst;

    // 싱글톤 초기화 및 이펙트 풀 선생성
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

    // 지정 위치에서 경험치 이펙트 재생
    public void PlayEffect(Vector3 pos)
    {
        GameObject obj = pool.Dequeue();
        obj.transform.position = pos;
        obj.SetActive(true);

        // 일정 시간 후 비활성화
        StartCoroutine(ReturnRoutine(obj, 0.5f));

        // 즉시 다시 풀에 등록 (비활성화는 코루틴에서 처리)
        pool.Enqueue(obj);
    }

    // 일정 시간 후 이펙트를 끄는 코루틴
    IEnumerator ReturnRoutine(GameObject obj, float time)
    {
        yield return new WaitForSeconds(time);
        obj.SetActive(false);
    }
}
