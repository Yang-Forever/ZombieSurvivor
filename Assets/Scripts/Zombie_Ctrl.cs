using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;
using UnityEditor;

public enum ZombieType
{
    Normal,
    Fast,
    Boss,
    None
}

public class Zombie_Ctrl : MonoBehaviour
{
    public ZombieType zomType = ZombieType.None;

    [Header("Base Stat")]
    [SerializeField] float baseHp;
    [SerializeField] float baseMoveSpeed;
    [SerializeField] int baseDamage;

    [Header("Runtime Stat")]
    float maxHp;
    float currHp;
    float moveSpeed;
    int damage;

    [Header("Multiplier")]
    public static float HpMultiplier = 1f;
    public static float SpeedMultiplier = 1f;
    public static float DamageMultiplier = 1f;

    private float atkRange = 2.0f;
    private float atkCool = 1.0f;
    float atkTimer = 0.0f;

    // 현재 상태
    AnimState state = AnimState.idle;
    [HideInInspector] public AnimState curState = AnimState.idle;
    [HideInInspector] public bool isDead = false;
    bool hasAttacked = false;
    Player_Ctrl player;

    // 애니메이션
    public Anim anim;
    Animator animator = null;

    public Transform target;

    private ZombiePool pool;

    [Header("Boss State")]
    public Transform dashLine;
    public Canvas dashCanvas;
    public Image dashFill;
    public GameObject dashHitBox;

    // 패턴 타이머
    float patternTimer = 10f;
    float patternCool = 10f;

    // 돌진 차징
    float dashChargeTime = 1f;
    float dashChargeTimer = 0f;
    bool isChargingDash = false;

    // 돌진 실행
    bool isDashing = false;
    float dashSpeed = 7f;
    float dashTime = 1.5f;
    float dashTimer;
    Vector3 dashDir;

    // 거리 조건
    float SkillRange = 8f;

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
        player = target.GetComponent<Player_Ctrl>();
        animator = GetComponentInChildren<Animator>();
        currHp = baseHp;
    }

    // Update is called once per frame
    void Update()
    {
        if (isDead)
            return;

        if (atkTimer > 0.0f)
            atkTimer -= Time.deltaTime;

        if (zomType == ZombieType.Boss)
            BossUpdate();
        else
            ZombieMove();
    }

    void ZombieSetUp()
    {
        switch (zomType)
        {
            case ZombieType.Normal: // 기본 좀비
                {
                    baseHp = 60f;
                    baseMoveSpeed = 3.0f;
                    baseDamage = 10;
                    atkRange = 2f;
                }
                break;

            case ZombieType.Fast:   // 속도(이동속도)가 빠른 좀비
                {
                    baseHp = 40f;
                    baseMoveSpeed = 4.0f;
                    baseDamage = 10;
                    atkRange = 2f;
                }
                break;

            case ZombieType.Boss:   // 보스
                {
                    baseHp = 400f;
                    baseMoveSpeed = 3.0f;
                    baseDamage = 20;
                    atkRange = 3f;
                }
                break;
        }
    }

    void ApplyDifficultyStat()
    {
        maxHp = baseHp * HpMultiplier;
        currHp = maxHp;

        moveSpeed = baseMoveSpeed * SpeedMultiplier;
        damage = Mathf.RoundToInt(baseDamage * DamageMultiplier);
    }

    #region Zombie Action
    void ZombieMove()
    {
        if (isDead)
            return;

        if (state == AnimState.attack)
            return;

        if (state == AnimState.rage)
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

        }
    }

    void ZombieAttack()
    {
        if (isDashing || isChargingDash)
            return;

        if (atkTimer > 0f)
            return;

        atkTimer = atkCool;
        hasAttacked = false;

        ChangeAnim(AnimState.attack, 0.12f);
    }

    public void OnAtkHit()
    {
        if (hasAttacked || isDead || player == null)
            return;

        float dist = Vector3.Distance(transform.position, player.transform.position);
        if (dist > atkRange)
            return;

        Vector3 dirToPlayer = (player.transform.position - transform.position).normalized;
        float dot = Vector3.Dot(transform.forward, dirToPlayer);

        if (dot < 0.5f)
            return;

        hasAttacked = true;
        if (player != null)
            player.HitDamage(damage);
    }

    public void OnAttackEnd()
    {
        ChangeAnim(AnimState.trace, 0.12f);
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

    #region Boss Action
    void BossUpdate()
    {
        float dist = Vector3.Distance(transform.position, target.position);

        // 대쉬 중
        if (isDashing)
        {
            DashUpdate();
            return;
        }

        // 대쉬 차징 중
        if (isChargingDash)
        {
            DashChargeUpdate();
            return;
        }

        // 기본 이동
        ZombieMove();

        // 스킬 범위 안에서만 패턴 카운트
        if (dist <= SkillRange)
        {
            patternTimer -= Time.deltaTime;

            if (patternTimer <= 0f)
                StartDashCharge();
        }
        else
        {
            // 멀어지면 쿨타임 리셋
            patternTimer = patternCool;
        }
    }

    void StartDashCharge()
    {
        patternTimer = patternCool;

        isChargingDash = true;
        dashChargeTimer = 0f;

        dashFill.fillAmount = 0f;
        dashCanvas.gameObject.SetActive(true);

        Vector3 dir = target.position - transform.position;
        dir.y = 0f;
        dashDir = dir.normalized;

        dashLine.transform.forward = dashDir;

        dashCanvas.transform.localRotation = Quaternion.Euler(90f, -90f, 0f);

        ChangeAnim(AnimState.rage, 0.12f);
    }

    void DashChargeUpdate()
    {
        dashChargeTimer += Time.deltaTime;
        dashFill.fillAmount = dashChargeTimer / dashChargeTime;

        if (dashFill.fillAmount >= 1f)
            StartDash();
    }

    void StartDash()
    {
        isChargingDash = false;
        isDashing = true;

        dashTimer = dashTime;
        dashCanvas.gameObject.SetActive(false);
        dashHitBox.SetActive(true);

        NavMeshAgent agent = GetComponent<NavMeshAgent>();
        if (agent == true)
        {
            agent.isStopped = true;
            agent.updatePosition = false;
            agent.updateRotation = false;
            agent.obstacleAvoidanceType = ObstacleAvoidanceType.NoObstacleAvoidance;
            agent.velocity = Vector3.zero;
            agent.avoidancePriority = 0;
        }

        gameObject.layer = LayerMask.NameToLayer("BossDash");

        ChangeAnim(AnimState.dash, 0.12f);
    }

    void DashUpdate()
    {
        dashTimer -= Time.deltaTime;

        transform.position += dashDir * dashSpeed * Time.deltaTime;

        if (dashTimer <= 0f)
            EndDash();
    }

    public void EndDash()
    {
        isDashing = false;
        dashHitBox.SetActive(false);

        NavMeshAgent agent = GetComponent<NavMeshAgent>();
        if (agent)
        {
            agent.Warp(transform.position);
            agent.isStopped = false;
            agent.updatePosition = true;
            agent.updateRotation = true;
            agent.obstacleAvoidanceType = ObstacleAvoidanceType.HighQualityObstacleAvoidance;
            agent.avoidancePriority = 10;
        }

        gameObject.layer = LayerMask.NameToLayer("Zombie");

        ChangeAnim(AnimState.trace, 0.12f);
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

            if (zomType == ZombieType.Boss)
            {
                SpawnExp(100);
                GameMgr.Inst.KillZombie(100);
                StartCoroutine(BossDie());
            }
            else
            {
                SpawnExp(10);
                GameMgr.Inst.KillZombie(10);
                StartCoroutine(Die());
            }
        }
        else
        {
            if (state == AnimState.attack || state == AnimState.dash || state == AnimState.rage)
                return;

            ChangeAnim(AnimState.hit, 0.12f);
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

    IEnumerator BossDie()
    {
        ChangeAnim(AnimState.die, 0.12f);

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb)
            rb.isKinematic = true;

        Collider col = GetComponent<Collider>();
        if (col)
            col.enabled = false;

        NavMeshAgent nav = GetComponent<NavMeshAgent>();
        if (nav)
            nav.enabled = false;

        yield return new WaitForSeconds(3f);

        Destroy(gameObject);
    }

    public void ResetZombie()
    {
        ApplyDifficultyStat();
        ZombieSetUp();

        atkTimer = 0.0f;
        hasAttacked = false;

        if (zomType == ZombieType.Boss)
        {
            isChargingDash = false;
            isDashing = false;

            dashCanvas.gameObject.SetActive(false);
            patternTimer = patternCool;
        }

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

    private void OnCollisionEnter(Collision collision)
    {
        if (!(zomType == ZombieType.Boss))
            return;

        if (!isDashing)
            return;

        if (collision.gameObject.CompareTag("Wall") ||
            collision.gameObject.CompareTag("House") ||
            collision.gameObject.CompareTag("Fence"))
        {
            EndDash();
        }
    }
}
