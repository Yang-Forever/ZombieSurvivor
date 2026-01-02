using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public enum ZombieType
{
    Normal,
    Fast,
    Tank,
    Boss,
    None
}

public class Zombie_Ctrl : MonoBehaviour
{
    public ZombieType zomType = ZombieType.None;

    [Header("State")]
    private float initHp = 100;
    private float currHp = 100;
    private int damage = 10;
    private float moveSpeed = 3.0f;
    private float atkRange = 2.0f;

    // 현재 상태
    AnimState state = AnimState.idle;
    [HideInInspector] public AnimState curState = AnimState.idle;
    AnimState aiState = AnimState.idle;
    [HideInInspector] public bool isDead = false;

    // 애니메이션
    public Anim anim;
    Animator animator = null;

    public Transform target;

    private ZombiePool pool;

    private void Awake()
    {
        ZombieSetUp();
    }

    public void SetPool(ZombiePool p)
    {
        pool = p;
    }

    // Start is called before the first frame update
    void Start()
    {
        target = GameObject.Find("Player").transform;
        animator = GetComponentInChildren<Animator>();
        currHp = initHp;
    }

    // Update is called once per frame
    void Update()
    {
        ZombieMove();
    }

    void ZombieSetUp()
    {
        switch (zomType)
        {
            case ZombieType.Normal: // 기본 좀비
                {
                    initHp = 60;
                    damage = 10;
                    moveSpeed = 3.0f;
                    atkRange = 1.3f;
                }
                break;

            case ZombieType.Fast:   // 속도(이동속도)가 빠른 좀비
                {
                    initHp = 40;
                    damage = 10;
                    moveSpeed = 5.0f;
                    atkRange = 2.0f;
                }
                break;

            case ZombieType.Tank:   // 체력이 많은 좀비
                {
                    initHp = 200;
                    damage = 10;
                    moveSpeed = 2.0f;
                    atkRange = 1.5f;
                }
                break;

            case ZombieType.Boss:   // 보스
                {

                }
                break;
        }
    }

    #region Zombie Action
    void ZombieMove()
    {
        if (isDead)
            return;

        Vector3 dir = target.position - transform.position;
        dir.y = 0;

        float dist = dir.magnitude;

        if (dist > atkRange)
        {
            dir.Normalize();
            transform.forward = dir;
            transform.position += dir * moveSpeed * Time.deltaTime;

            ChangeAnim(AnimState.trace, 0.12f);
        }
        else
        {
            // 좀비 공격
            ZombieAttack();
            ChangeAnim(AnimState.attack, 0.12f);
        }
    }

    void ZombieAttack()
    {

    }

    void ChangeAnim(AnimState newState, float crossTime = 0.0f)
    {
        if (state == newState)
            return;

        if (animator != null)
        {
            animator.ResetTrigger(state.ToString());

            if (0.0f < crossTime)
                animator.SetTrigger(newState.ToString());
            else
            {
                string strAnim = anim.Idle.name;
                animator.Play(strAnim, -1, 0);
            }
        }

        state = newState;
        curState = newState;
    }

    #endregion


    public void HitDamage(float damage)
    {
        if (currHp <= 0)
            return;

        currHp -= damage;

        if (currHp <= 0)
        {
            currHp = 0;
            isDead = true;

            SpawnExp(10);

            StartCoroutine(Die());
        }
    }

    IEnumerator Die()
    {
        // 이미 죽음 상태라면 중복 처리 방지
        if (state == AnimState.die)
            yield break;

        // 1. 애니메이션 "Die" 재생
        ChangeAnim(AnimState.die, 0.12f);

        // 2. 물리 제거
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb)
            rb.isKinematic = true;

        // 3. 콜라이더 비활성화
        Collider col = GetComponent<Collider>();
        if (col)
            col.enabled = false;

        // 4. 네비게이션 매쉬 비활성화
        NavMeshAgent nav = GetComponent<NavMeshAgent>();
        if (nav)
            nav.enabled = false;

        // 5. 삭제
        yield return new WaitForSeconds(3f);

        pool.ReturnZombie(this);
    }

    public void ResetZombie()
    {
        currHp = initHp;

        ChangeAnim(AnimState.idle);
        state = AnimState.idle;
        curState = AnimState.idle;

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = false;

        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = true;

        NavMeshAgent agent = GetComponent<NavMeshAgent>();
        if (agent != null)
        {
            agent.enabled = true;
            agent.ResetPath();
        }

        isDead = false;
    }

    void SpawnExp(int value)
    {
        ExpObj_Ctrl exp = ExpPool.Inst.OnGetExp();

        Vector3 pos = transform.position;
        pos.y = 0.5f;

        exp.transform.position = pos;

        exp.SetUpExp(value);
    }

}
