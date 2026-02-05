using UnityEngine;

/// <summary>
/// 회전하며 대기
/// 자석 범위 진입 시 플레이어에게 끌려감
/// 플레이어 접촉 시 경험치 휙득 및 사운드 출력 후 풀로 반환
/// </summary>
public class ExpObj_Ctrl : MonoBehaviour
{
    private Transform player;
    private Player_Ctrl playerCtrl;

    float rotSpeed = 60.0f;
    private float moveSpeed = 30f;

    private int expValue = 10;

    bool isMagnet = false;  // 자석 상태 여부

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
            // 자석 상태가 아닐 때 회전
            transform.Rotate(0, rotSpeed * Time.deltaTime, 0);

            // 플레이어 자석 범위 진입 시 자석 상태 전환
            if (dist <= PlayerStats.Inst.MagnetRange)
            {
                isMagnet = true;
            }
        }
    }

    // 플레이어 방향으로 끌려가는 이동
    void MagnetPlayer()
    {
        Vector3 dir = (player.position - transform.position).normalized;
        transform.position += dir * moveSpeed * Time.deltaTime;
    }

    // 경험치 값 설정
    public void SetUpExp(int value)
    {
        expValue = value;
    }

    // 플레이어 충돌 시 경험치 획득 처리
    private void OnTriggerEnter(Collider coll)
    {
        if (coll.CompareTag("Player"))
        {
            //플레이어 Exp 휙득
            coll.GetComponent<Player_Ctrl>().AddExp(expValue);
            Sound_Mgr.Inst.PlayEffSoundLimit("ExpHit", 0.5f, 0.05f);
            ExpEffectPool.Inst.PlayEffect(transform.position);

            // 풀로 반환
            ExpPool.Inst.ReturnExp(this);
        }
    }
}
