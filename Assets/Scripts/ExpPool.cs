using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExpPool : MonoBehaviour
{
    public GameObject ExpPrefab = null;
    private Queue<ExpObj_Ctrl> expPool = new Queue<ExpObj_Ctrl>();
    public Transform expSpawner;

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
        var obj = Instantiate(ExpPrefab).GetComponent<ExpObj_Ctrl>();
        obj.transform.SetParent(expSpawner);
        obj.gameObject.SetActive(false);
        expPool.Enqueue(obj);
    }

    public ExpObj_Ctrl OnGetExp()
    {
        if (expPool.Count == 0)
        {
            CreateExp();
        }

        ExpObj_Ctrl exp = expPool.Dequeue();
        exp.gameObject.SetActive(true);

        exp.transform.SetParent(expSpawner);

        return exp;
    }

    public void ReturnExp(ExpObj_Ctrl obj)
    {
        obj.gameObject.SetActive(false);
        expPool.Enqueue(obj);
    }
}
