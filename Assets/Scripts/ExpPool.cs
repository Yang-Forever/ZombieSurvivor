using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExpPool : MonoBehaviour
{
    public GameObject expPrefab = null;
    public GameObject bossExpPrefab = null;

    private Queue<ExpObj_Ctrl> expPool = new Queue<ExpObj_Ctrl>();
    public Transform exps;

    private int expPoolCount = 100;

    public static ExpPool Inst = null;

    private void Awake()
    {
        Inst = this;

        for (int i = 0; i < expPoolCount; i++)
        {
            CreateExp();
        }
    }
    void CreateExp()
    {
        var obj = Instantiate(expPrefab).GetComponent<ExpObj_Ctrl>();
        obj.transform.SetParent(exps);
        obj.gameObject.SetActive(false);
        expPool.Enqueue(obj);
    }
    public ExpObj_Ctrl CreateBossExp()
    {
        ExpObj_Ctrl exp = Instantiate(bossExpPrefab).GetComponent<ExpObj_Ctrl>();
        exp.transform.SetParent(exps);
        return exp;
    }

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

    public void ReturnExp(ExpObj_Ctrl obj)
    {
        obj.gameObject.SetActive(false);
        expPool.Enqueue(obj);
    }
}
