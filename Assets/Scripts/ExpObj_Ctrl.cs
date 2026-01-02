using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExpObj_Ctrl : MonoBehaviour
{
    private Transform player;
    private Player_Ctrl playerCtrl;

    float rotSpeed = 60.0f;
    private float moveSpeed = 15f;

    private int expValue = 10;

    bool isMagnet = false;

    void OnEnable()
    {
        isMagnet = false;
    }

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        playerCtrl = player.GetComponent<Player_Ctrl>();
    }

    // Update is called once per frame
    void Update()
    {
        float dist = Vector3.Distance(transform.position, player.position);

        if (isMagnet)
        {
            MagnetPlayer();
            return;
        }
        else
        {
            transform.Rotate(0, rotSpeed * Time.deltaTime, 0);

            if (dist <= PlayerStats.Inst.MagnetRange)
            {
                isMagnet = true;
            }
        }
    }

    void MagnetPlayer()
    {
        Vector3 dir = (player.position - transform.position).normalized;
        transform.position += dir * moveSpeed * Time.deltaTime;
    }

    public void SetUpExp(int value)
    {
        expValue = value;
    }

    private void OnTriggerEnter(Collider coll)
    {
        if (coll.CompareTag("Player"))
        {
            //ÇÃ·¹ÀÌ¾î Exp È×µæ
            coll.GetComponent<Player_Ctrl>().AddExp(expValue);

            ExpPool.Inst.ReturnExp(this);
        }
    }
}
