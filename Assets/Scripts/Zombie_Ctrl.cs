using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public enum ZombieType
{
    Normal,
    Explosion,
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

    [SerializeField] float moveSpeed2 = 3f;
    [SerializeField] float stopDistance = 1.6f;
    [SerializeField] float personalSpace = 0.9f;

    [Header("Multiplier")]
    public static float NormalHpMul = 1f;
    public static float NormalSpeedMul = 1f;
    public static float NormalDmgMul = 1f;

    public static float BossHpMul = 1f;
    public static float BossSpeedMul = 1f;
    public static float BossDmgMul = 1f;

    private float atkRange = 2.0f;
    private float atkCool = 1.0f;
    float atkTimer = 0.0f;
    bool isAttack = false;

    // 현재 상태
    private ZombiePool pool;
    AnimState state = AnimState.idle;
    [HideInInspector] public AnimState curState = AnimState.idle;
    [HideInInspector] public bool isDead = false;
    bool hasAttacked = false;

    // 타겟(플레이어) 참조
    public Transform target;
    Player_Ctrl player;

    // 애니메이션
    public Anim anim;
    Animator animator = null;

    // 이동
    [SerializeField] LayerMask zombieLayer;
    Rigidbody rb;
    bool wasBlockedLastFrame = false;

    bool isInit = false;

    [Header("Bomb Zombie State")]
    public GameObject explodeEffect;
    bool isExplosion = false;

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

    // 피격 관련
    Renderer[] renderers;
    MaterialPropertyBlock mpb;
    Coroutine hitCo;

    Color baseColor;
    Color hitColor = Color.red;


    private void Awake()
    {
        animator = GetComponentInChildren<Animator>();
        rb = GetComponent<Rigidbody>();

        renderers = GetComponentsInChildren<SkinnedMeshRenderer>();
        mpb = new MaterialPropertyBlock();

        if (renderers.Length > 0)
            baseColor = renderers[0].sharedMaterial.GetColor("_Color");
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
    }

    // Update is called once per frame
    void Update()
    {
        if (!isInit)
            return;

        if (isDead)
            return;

        if (atkTimer > 0.0f)
            atkTimer -= Time.deltaTime;

        if (zomType == ZombieType.Boss)
            BossMove();
        else if (zomType == ZombieType.Explosion)
            BombZombieMove();
        else if (zomType == ZombieType.Normal)
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

            case ZombieType.Explosion:
                {
                    baseHp = 40f;
                    baseMoveSpeed = 5.0f;
                    baseDamage = 30;
                    atkRange = 1f;
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
        if (zomType == ZombieType.Boss)
        {
            maxHp = baseHp * BossHpMul;
            moveSpeed = baseMoveSpeed * BossSpeedMul;
            damage = Mathf.RoundToInt(baseDamage * BossDmgMul);
        }
        else
        {
            maxHp = baseHp * NormalHpMul;
            moveSpeed = baseMoveSpeed * NormalSpeedMul;
            damage = Mathf.RoundToInt(baseDamage * NormalDmgMul);
        }

        currHp = maxHp;
    }

    #region Zombie Action

    void ZombieMove()
    {
        if (state == AnimState.attack)
            return;

        Vector3 toPlayer = target.position - transform.position;
        toPlayer.y = 0f;

        float dist = toPlayer.magnitude;
        if (dist < 0.01f)
            return;

        Vector3 dir = toPlayer.normalized;

        if (dist <= atkRange)
        {
            ZombieAttack();
            return;
        }

        bool isBlocked = IsBlockedByFrontZombie(dir);

        if (wasBlockedLastFrame && !isBlocked)
        {
            dir = ApplyReleaseBias(dir);
        }

        wasBlockedLastFrame = isBlocked;

        if (isBlocked)
        {
            ChangeAnim(AnimState.idle, 0.1f);
            return;
        }

        transform.position += dir * moveSpeed * Time.deltaTime;
        transform.forward = dir;

        ChangeAnim(AnimState.trace, 0.12f);
    }


    Vector3 ApplyReleaseBias(Vector3 dir)
    {
        // 좌/우 중 랜덤
        Vector3 side = Vector3.Cross(Vector3.up, dir);
        float sign = Random.value < 0.5f ? -1f : 1f;

        float biasStrength = 0.25f;

        Vector3 newDir = (dir + side * sign * biasStrength).normalized;
        return newDir;
    }


    bool IsBlockedByFrontZombie(Vector3 moveDir)
    {
        // Normal만 차단 로직 사용
        if (zomType != ZombieType.Normal)
            return false;

        Collider[] cols = Physics.OverlapCapsule(
            transform.position + Vector3.up * 0.5f,
            transform.position + Vector3.up * 1.5f,
            0.4f,
            zombieLayer
        );

        float myDist = Vector3.Distance(transform.position, target.position);

        foreach (var col in cols)
        {
            if (col.attachedRigidbody == rb)
                continue;

            Zombie_Ctrl other = col.GetComponent<Zombie_Ctrl>();
            if (other == null || other.isDead)
                continue;

            if (other.zomType != ZombieType.Normal)
                continue;

            float otherDist = Vector3.Distance(other.transform.position, target.position);

            if (otherDist < myDist)
            {
                Vector3 toOther =
                    (other.transform.position - transform.position).normalized;

                if (Vector3.Dot(moveDir, toOther) > 0.3f)
                    return true;
            }
        }

        return false;
    }


    void BombZombieMove()
    {
        if (isDead || isExplosion)
            return;

        Vector3 dir = target.position - transform.position;
        dir.y = 0f;

        float dist = dir.magnitude;
        if (dist < 0.01f)
            return;

        dir.Normalize();

        if (dist <= atkRange)
        {
            Explosion();
            return;
        }

        transform.position += dir * moveSpeed * Time.deltaTime;
        transform.forward = dir;
        PushThroughZombies(dir);

        ChangeAnim(AnimState.trace, 0.12f);
    }

    void PushThroughZombies(Vector3 moveDir)
    {
        float radius = 0.45f;
        float pushPower = 0.04f;

        Collider[] hits = Physics.OverlapCapsule(
            transform.position + Vector3.up * 0.5f,
            transform.position + Vector3.up * 1.5f,
            radius,
            zombieLayer
        );

        foreach (var hit in hits)
        {
            Zombie_Ctrl z = hit.GetComponentInParent<Zombie_Ctrl>();
            if (z == null || z.isDead)
                continue;

            if (z.zomType != ZombieType.Normal)
                continue;

            Vector3 toZombie = (z.transform.position - transform.position);
            toZombie.y = 0f;

            if (Vector3.Dot(moveDir, toZombie.normalized) < 0.2f)
                continue;

            z.transform.position += moveDir * pushPower;
        }
    }

    void Explosion()
    {
        if (isExplosion)
            return;

        isExplosion = true;
        isDead = true;

        Sound_Mgr.Inst.PlayEffSoundLimit("ZombieExplosion", 1.0f, 0.1f);

        if (player != null)
            player.HitDamage(damage);

        // 폭발 이펙트 추가
        GameObject efx = Instantiate(explodeEffect, transform.position, Quaternion.identity);
        Destroy(efx, 0.3f);

        pool.ReturnZombie(this);
    }

    void ZombieAttack()
    {
        if (state == AnimState.attack)
            return;

        if (atkTimer > 0f)
            return;

        atkTimer = atkCool;
        hasAttacked = false;

        state = AnimState.attack;
        curState = AnimState.attack;

        ChangeAnim(AnimState.attack, 0.12f);
    }

    public void OnAtkHit()
    {
        if (state != AnimState.attack)
            return;

        if (hasAttacked || isDead || player == null)
            return;

        float dist = Vector3.Distance(transform.position, player.transform.position);
        if (dist > atkRange + 0.2f)
            return;

        Vector3 dirToPlayer = (player.transform.position - transform.position).normalized;
        if (Vector3.Dot(transform.forward, dirToPlayer) < 0.3f)
            return;

        hasAttacked = true;
        player.HitDamage(damage);
    }

    public void OnAttackEnd()
    {
        state = AnimState.trace;
        curState = AnimState.trace;
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
    void BossMove()
    {
        if (isDashing)
        {
            DashUpdate();
            return;
        }

        if (isChargingDash)
        {
            DashChargeUpdate();
            return;
        }

        ZombieMove();

        float dist = Vector3.Distance(transform.position, target.position);

        if (dist <= SkillRange)
        {
            patternTimer -= Time.deltaTime;
            if (patternTimer <= 0f)
                StartDashCharge();
        }
        else
        {
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

        gameObject.layer = LayerMask.NameToLayer("Zombie");

        ChangeAnim(AnimState.trace, 0.12f);
    }
    #endregion


    public void HitDamage(float damage)
    {
        if (currHp <= 0)
            return;

        currHp -= damage;

        HitEffect();

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

    }

    void HitEffect()
    {
        if (hitCo != null)
            StopCoroutine(hitCo);

        hitCo = StartCoroutine(HitEffectCo());
    }

    IEnumerator HitEffectCo()
    {
        Hit(1f);
        yield return new WaitForSeconds(0.05f);
        Hit(0f);
    }

    void Hit(float v)
    {
        Color col = Color.Lerp(baseColor, hitColor, v);

        foreach (var r in renderers)
        {
            r.GetPropertyBlock(mpb);
            mpb.SetColor("_Color", col);
            r.SetPropertyBlock(mpb);
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

        // 4. 삭제
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

        yield return new WaitForSeconds(3f);

        Destroy(gameObject);
    }

    public void ResetZombie()
    {
        ZombieSetUp();
        ApplyDifficultyStat();

        isDead = false;
        isExplosion = false;
        atkTimer = 0f;
        hasAttacked = false;

        ChangeAnim(AnimState.idle);

        if (zomType == ZombieType.Boss)
        {
            isChargingDash = false;
            isDashing = false;
            dashCanvas.gameObject.SetActive(false);
            patternTimer = patternCool;
        }

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = false;
        }

        Collider col = GetComponent<Collider>();
        if (col)
            col.enabled = true;

        isInit = true;
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

        if (collision.gameObject.CompareTag("Wall"))
        {
            EndDash();
        }
    }
}
