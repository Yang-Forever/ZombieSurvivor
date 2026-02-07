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

/// <summary>
/// 좀비의 이동, 공격, 피격, 사망 및 보스 패턴까지 포함한 전체 행동을 제어
/// </summary>
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

    [Header("Attack")]
    float atkRange = 2.0f;
    float atkCool = 1.0f;
    float atkTimer = 0.0f;
    float hitCheckWindow = 0.15f;
    float hitCheckTimer = 0f;

    bool hasAttacked = false;

    // 현재 상태
    private ZombiePool pool;
    AnimState state = AnimState.idle;
    [HideInInspector] public AnimState curState = AnimState.idle;
    [HideInInspector] public bool isDead = false;

    // 타겟(플레이어) 참조
    public Transform target;
    Player_Ctrl player;

    // 애니메이션
    public Anim anim;
    Animator animator = null;
    SkinnedMeshRenderer[] skMeshRenderer;
    float lodNear = 20f;
    float lodMid = 23f;

    // 이동
    Vector3 moveDir;
    Vector3 sepDir;
    bool wantMove;

    [Header("Map Bounds")]
    public BoxCollider mapBounds;
    float mapPadding = 0.6f;

    [SerializeField] LayerMask zombieLayer;
    static Collider[] overlapCache = new Collider[32];
    int separationOffset;
    int resolveFrameOffset;

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

    [Header("Hit")]
    MaterialPropertyBlock mpb;
    Coroutine hitCo;

    Color baseColor;
    Color hitColor = Color.red;

    // 컴포넌트 및 초기 상태 설정
    private void Awake()
    {
        animator = GetComponentInChildren<Animator>();

        skMeshRenderer = GetComponentsInChildren<SkinnedMeshRenderer>();
        mpb = new MaterialPropertyBlock();

        if (skMeshRenderer.Length > 0)
            baseColor = skMeshRenderer[0].sharedMaterial.GetColor("_Color");

        separationOffset = Random.Range(0, 3);
        resolveFrameOffset = Random.Range(0, 3);
    }

    // 플레이어 및 맵 참조 설정
    void Start()
    {
        target = GameObject.Find("Player").transform;
        player = target.GetComponent<Player_Ctrl>();
        mapBounds = GameObject.Find("MapBounds").GetComponent<BoxCollider>();
    }

    // 물리 이동 처리
    private void FixedUpdate()
    {
        if (!isInit || isDead || !wantMove)
            return;

        Vector3 move = moveDir * moveSpeed + sepDir * 0.6f;   // 분리 영향도 조절

        Vector3 nextPos = transform.position + move * Time.fixedDeltaTime;

        ClampToMap(ref nextPos);

        transform.position = nextPos;
    }

    // 상태별 행동 처리
    void Update()
    {
        if (GameMgr.Inst.state != GameState.Play)
            return;

        if (!isInit || isDead)
            return;

        UpdateLOD();

        if (atkTimer > 0.0f)
            atkTimer -= Time.deltaTime;
        else
            atkTimer = 0.0f;

        if (hitCheckTimer > 0f)
        {
            hitCheckTimer -= Time.deltaTime;
            CheckAtkHit();
        }

        if (zomType == ZombieType.Boss)
            BossMove();
        else if (zomType == ZombieType.Explosion)
            BombZombieMove();
        else if (zomType == ZombieType.Normal)
            ZombieMove();
    }

    #region ZombieSetUp
    // 풀 참조 설정
    public void SetPool(ZombiePool p)
    {
        pool = p;
    }

    // 좀비 타입별 기본 스탯 설정
    void ZombieSetUp()
    {
        switch (zomType)
        {
            case ZombieType.Normal: // 기본 좀비
                {
                    baseHp = 60f;
                    baseMoveSpeed = 2.0f;
                    baseDamage = 10;
                    atkRange = 1.5f;
                }
                break;

            case ZombieType.Explosion:
                {
                    baseHp = 40f;
                    baseMoveSpeed = 3.5f;
                    baseDamage = 30;
                    atkRange = 1f;
                }
                break;

            case ZombieType.Boss:   // 보스
                {
                    baseHp = 400f;
                    baseMoveSpeed = 3.0f;
                    baseDamage = 20;
                    atkRange = 2f;
                }
                break;
        }
    }

    // 난이도 보정 스탯 적용
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

    // 풀에서 재사용 시 초기화
    public void ResetZombie()
    {
        ZombieSetUp();
        ApplyDifficultyStat();

        isDead = false;
        isExplosion = false;
        atkTimer = 0f;
        hasAttacked = false;
        hitCheckTimer = 0f;

        ChangeAnim(AnimState.idle);

        ResetHitColor();

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

    #endregion

    #region Zombie Action
    // 일반 좀비 이동 로직
    void ZombieMove()
    {
        wantMove = false;

        if (isDead || isExplosion)
            return;

        if (state == AnimState.attack)
        {
            RotateToPlayer();
            return;
        }

        Vector3 toPlayer = target.position - transform.position;
        toPlayer.y = 0f;

        float dist = toPlayer.magnitude;

        if (dist <= atkRange)
        {
            ZombieAttack();
            return;
        }

        moveDir = toPlayer.normalized;
        sepDir = GetSeparationVector();
        wantMove = true;

        RotateToPlayer();
        ChangeAnim(AnimState.trace, 0.12f);
    }

    // 밀집 회피 벡터 계산
    Vector3 GetSeparationVector()
    {
        float radius = 0.6f;
        float strength = 0.02f;

        int count = Physics.OverlapSphereNonAlloc(transform.position, radius, overlapCache, zombieLayer);

        if (count <= 1)
            return Vector3.zero;

        Vector3 sep = Vector3.zero;

        for (int i = 0; i < count; i++)
        {
            Zombie_Ctrl other = overlapCache[i].GetComponentInParent<Zombie_Ctrl>();
            if (other == null || other == this || other.isDead)
                continue;

            if (other.zomType != ZombieType.Normal)
                continue;

            Vector3 diff = transform.position - other.transform.position;
            diff.y = 0f;

            float d = diff.magnitude;
            if (d < 0.001f)
                continue;

            Vector3 forward = (target.position - transform.position);
            forward.y = 0f;
            forward.Normalize();

            Vector3 side = diff - Vector3.Project(diff, forward);

            sep += side.normalized * Mathf.Clamp01(1f - d / radius);
        }

        float strengthScale = Mathf.Clamp01(1f - (count / 10f));

        Vector3 result = sep.normalized * strength * strengthScale;
        return Vector3.ClampMagnitude(result, 0.15f);

    }

    // 맵 범위 제한
    void ClampToMap(ref Vector3 pos)
    {
        if (!mapBounds)
            return;

        Bounds b = mapBounds.bounds;

        pos.x = Mathf.Clamp(pos.x, b.min.x + mapPadding, b.max.x - mapPadding);
        pos.z = Mathf.Clamp(pos.z, b.min.z + mapPadding, b.max.z - mapPadding);
    }

    // 폭탄 좀비 이동
    void BombZombieMove()
    {
        wantMove = false;

        if (isDead || isExplosion)
            return;

        Vector3 toPlayer = target.position - transform.position;
        toPlayer.y = 0f;

        float dist = toPlayer.magnitude;

        if (dist <= atkRange)
        {
            Explosion();
            return;
        }

        moveDir = toPlayer.normalized;
        sepDir = Vector3.zero;
        wantMove = true;

        RotateToPlayer();
        ChangeAnim(AnimState.trace, 0.12f);
    }

    // 플레이어 방향 회전
    void RotateToPlayer()
    {
        if (!target) return;

        Vector3 dir = target.position - transform.position;
        dir.y = 0f;

        if (dir.sqrMagnitude < 0.001f)
            return;

        Quaternion targetRot = Quaternion.LookRotation(dir);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot,Time.deltaTime * 10f);
    }

    // 폭발 처리
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

    // 공격 시작 처리
    void ZombieAttack()
    {
        if (state == AnimState.attack)
            return;

        if (atkTimer > 0f)
            return;

        atkTimer = atkCool;
        hasAttacked = false;
        hitCheckTimer = 0f;

        Vector3 dir = target.position - transform.position;
        dir.y = 0f;

        if (dir.sqrMagnitude > 0.001f)
            transform.rotation = Quaternion.LookRotation(dir);

        ChangeAnim(AnimState.attack, 0.12f);
    }

    // 공격 히트 이벤트
    public void OnAtkHit()
    {
        if (state != AnimState.attack)
            return;

        if (hasAttacked || isDead || player == null)
            return;

        hitCheckTimer = hitCheckWindow;
    }

    // 애니메이션 타이밍 실제 히트 판정
    void CheckAtkHit()
    {
        if(hasAttacked || isDead || player == null)
        {
            hitCheckTimer = 0f;
            return;
        }

        float dist = Vector3.Distance(transform.position, player.transform.position);

        if (dist > atkRange + 0.5f)
            return;

        Vector3 dirToPlayer = (player.transform.position - transform.position);
        dirToPlayer.y = 0f;

        float dot = Vector3.Dot(transform.forward, dirToPlayer);
        if (dot < -0.3f)
            return;

        hasAttacked = true;
        hitCheckTimer = 0f;
        player.HitDamage(damage);
    }

    // 공격 종료 이벤트
    public void OnAttackEnd()
    {
        ChangeAnim(AnimState.trace, 0.12f);
    }

    // 애니메이션 전환 처리
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
    // 보스 행동 메인 루프
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

        BossLeadMove();

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

    // 보스 기본 추적 이동
    void BossLeadMove()
    {
        wantMove = false;

        if (state == AnimState.attack)
            return;

        Vector3 toPlayer = target.position - transform.position;
        toPlayer.y = 0f;

        float dist = toPlayer.magnitude;

        if (dist <= atkRange)
        {
            ZombieAttack();
            return;
        }

        moveDir = toPlayer.normalized;
        sepDir = Vector3.zero;   // 보스는 밀집 계산 제거
        wantMove = true;

        RotateToPlayer();
        ChangeAnim(AnimState.trace, 0.12f);
    }

    // 돌진 차징 상태 업데이트
    void DashChargeUpdate()
    {
        wantMove = false;
        moveDir = Vector3.zero;
        sepDir = Vector3.zero;

        dashChargeTimer += Time.deltaTime;
        dashFill.fillAmount = dashChargeTimer / dashChargeTime;

        if (dashFill.fillAmount >= 1f)
            StartDash();
    }

    // 돌진 차징 시작
    void StartDashCharge()
    {
        patternTimer = patternCool;

        isChargingDash = true;
        dashChargeTimer = 0f;

        wantMove = false;
        moveDir = Vector3.zero;
        sepDir = Vector3.zero;

        dashFill.fillAmount = 0f;
        dashCanvas.gameObject.SetActive(true);

        Vector3 dir = target.position - transform.position;
        dir.y = 0f;
        dashDir = dir.normalized;

        dashLine.transform.forward = dashDir;

        dashCanvas.transform.localRotation = Quaternion.Euler(90f, -90f, 0f);

        ChangeAnim(AnimState.rage, 0.12f);
    }

    // 돌진 실행 시작
    void StartDash()
    {
        isChargingDash = false;
        isDashing = true;

        dashTimer = dashTime;
        dashCanvas.gameObject.SetActive(false);
        dashHitBox.SetActive(true);

        ChangeAnim(AnimState.dash, 0.12f);
    }

    // 돌진 중 이동 처리
    void DashUpdate()
    {
        wantMove = true;

        moveDir = dashDir;
        sepDir = Vector3.zero;

        dashTimer -= Time.deltaTime;

        if (dashTimer <= 0f)
            EndDash();
    }

    // 돌진 종료 처리
    public void EndDash()
    {
        isDashing = false;
        dashHitBox.SetActive(false);

        ChangeAnim(AnimState.trace, 0.12f);
    }
    #endregion

    #region Hit Action

    // 데미지 처리 및 사망 판정
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
                SpawnBossExp(300);
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

    // 피격 이펙트 시작
    void HitEffect()
    {
        if (hitCo != null)
        {
            StopCoroutine(hitCo);
            hitCo = null;
        }

        hitCo = StartCoroutine(HitEffectCo());
    }

    // 피격 색상 연출 코루틴
    IEnumerator HitEffectCo()
    {
        Hit(1f);
        yield return new WaitForSeconds(0.05f);
        Hit(0f);
    }

    // 히트 컬러 적용
    void Hit(float v)
    {
        Color col = Color.Lerp(baseColor, hitColor, v);

        foreach (var r in skMeshRenderer)
        {
            r.GetPropertyBlock(mpb);
            mpb.SetColor("_Color", col);
            r.SetPropertyBlock(mpb);
        }
    }

    // 히트 컬러 초기화
    void ResetHitColor()
    {
        foreach (var r in skMeshRenderer)
        {
            r.GetPropertyBlock(mpb);
            mpb.SetColor("_Color", baseColor);
            r.SetPropertyBlock(mpb);
        }
    }

    #endregion

    #region Die Action
    // 일반 좀비 사망 처리
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

    // 보스 사망 처리
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

    // 일반 경험치 생성
    void SpawnExp(int value)
    {
        ExpObj_Ctrl exp = ExpPool.Inst.OnGetExp();

        Vector3 pos = transform.position;
        pos.y = 0.5f;

        exp.transform.position = pos;

        exp.SetUpExp(value);
    }

    // 보스 경험치 생성
    void SpawnBossExp(int value)
    {
        ExpObj_Ctrl exp = ExpPool.Inst.CreateBossExp();

        Vector3 pos = transform.position;
        pos.y = 0.5f;

        exp.transform.position = pos;

        exp.SetUpExp(value);
    }

    #endregion

    #region Animation LOD
    // 스킨드 메시 렌더러 토글
    void SetSkinnedMesh(bool on)
    {
        foreach (var r in skMeshRenderer)
            r.enabled = on;
    }

    // 거리 기반 애니메이션 LOD 처리
    void UpdateLOD()
    {
        if (zomType == ZombieType.Boss)
            return;

        if (state == AnimState.attack)
            return;

        float dist = Vector3.Distance(transform.position, target.position);

        if (dist < lodNear)
        {
            animator.enabled = true;
            SetSkinnedMesh(true);
        }
        else if (dist < lodMid)
        {
            animator.enabled = false;
            SetSkinnedMesh(true);
        }
        else
        {
            animator.enabled = false;
            SetSkinnedMesh(false);
        }
    }

    #endregion

    // 보스 돌진 중 벽 충돌 시 돌진 종료 처리
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
