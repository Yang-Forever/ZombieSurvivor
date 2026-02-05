using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 경험치 오브젝트 풀
/// 
/// </summary>
public class ExpPool : MonoBehaviour
{
    public GameObject expPrefab = null;
    public GameObject bossExpPrefab = null;

    private Queue<ExpObj_Ctrl> expPool = new Queue<ExpObj_Ctrl>();
    public Transform exps;

    private int expPoolCount = 100;     // 초기 풀 생성 개수

    public static ExpPool Inst = null;

    // 싱글톤 초기화 및 경험치 풀 선생성
    private void Awake()
    {
        Inst = this;

        for (int i = 0; i < expPoolCount; i++)
        {
            CreateExp();
        }
    }

    // 일반 경험치 오브젝트 1개 생성 후 풀에 추가
    void CreateExp()
    {
        var obj = Instantiate(expPrefab).GetComponent<ExpObj_Ctrl>();
        obj.transform.SetParent(exps);
        obj.gameObject.SetActive(false);
        expPool.Enqueue(obj);
    }

    // 보스 경험치 생성 (풀링 없이 즉시 생성)
    public ExpObj_Ctrl CreateBossExp()
    {
        ExpObj_Ctrl exp = Instantiate(bossExpPrefab).GetComponent<ExpObj_Ctrl>();
        exp.transform.SetParent(exps);
        return exp;
    }

    // 경험치 오브젝트 요청
    public ExpObj_Ctrl OnGetExp()
    {
        if (expPool.Count == 0)
        {
            CreateExp();
        }

        ExpObj_Ctrl exp = expPool.Dequeue();
        exp.gameObject.SetActive(true);

        exp.transform.SetParent(exps);

        return exp;
    }

    // 경험치 회수 (플레이어 획득 후)
    public void ReturnExp(ExpObj_Ctrl obj)
    {
        obj.gameObject.SetActive(false);
        expPool.Enqueue(obj);
    }
}
