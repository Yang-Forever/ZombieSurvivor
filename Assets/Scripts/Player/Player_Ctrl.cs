using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 플레이어 입력, 이동, 회전, 피격, 경험치 및 레벨업을 담당
/// </summary>
public class Player_Ctrl : MonoBehaviour
{
    [Header("Player Move")]
    float h = 0.0f;
    float v = 0.0f;
    float moveBlockRatio = 1f;  // 주변 적 밀집도에 따른 이동 속도 감속 비율
    float moveCheckTimer = 0f;  // 이동 가능 여부 체크 추가용 타이머
    Vector3 moveDir;
    bool wantMove = false;

    [Header("Map Bounds")]
    public BoxCollider mapBounds;
    float mapPadding = 0.5f;

    [Header("Player Setting")]
    public Image expBar;
    float curExp = 0;
    float maxExp = 50;
    int level = 1;
    public Image hpBar;
    public Text hpText;
    [HideInInspector] public bool isDie = false;

    PlayerAnim_Ctrl playerAnim;

    public static Player_Ctrl Inst = null;

    private void Awake()
    {
        Inst = this;

        playerAnim = GetComponentInChildren<PlayerAnim_Ctrl>();
    }

    private void Start()
    {
        UpdateHpUI();
    }

    // 실제 이동 처리 (물리 프레임)
    void FixedUpdate()
    {
        if (GameMgr.Inst.state != GameState.Play || isDie || !wantMove)
            return;

        float speed = PlayerStats.Inst.MoveSpeed * moveBlockRatio;

        Vector3 nextPos = transform.position + moveDir * speed * Time.fixedDeltaTime;

        ClampToMap(ref nextPos);

        transform.position = nextPos;
    }

    // 입력 및 회전 처리
    void Update()
    {
        if (GameMgr.Inst.state != GameState.Play || isDie)
            return;

        MoveKB();
        RotateMouse();
    }

    // 키보드 입력 기반 이동 처리
    void MoveKB()
    {
        h = Input.GetAxisRaw("Horizontal");
        v = Input.GetAxisRaw("Vertical");

        Vector3 input = new Vector3(h, 0f, v);

        // 입력이 없으면 정지
        if (input.sqrMagnitude < 0.01f)
        {
            wantMove = false;
            playerAnim.MoveAnim(0, 0);
            return;
        }

        input.Normalize();

        // 일정 주기마다 이동 가능 여부 검사 (과도한 체크 방지)
        moveCheckTimer += Time.deltaTime;
        if (moveCheckTimer >= 0.1f)
        {
            moveCheckTimer = 0f;
            moveBlockRatio = CanMove(input) ? 1f : 0.5f;
        }

        moveDir = input;
        wantMove = true;

        // 로컬 좌표 기준 이동 애니메이션 값 계산
        Vector3 localMoveDir = transform.InverseTransformDirection(input);
        playerAnim.MoveAnim(localMoveDir.x, localMoveDir.z);
    }

    // 마우스 위치를 기준으로 플레이어 회전
    void RotateMouse()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        Plane plane = new Plane(Vector3.up, Vector3.zero);

        if (plane.Raycast(ray, out float distance))
        {
            Vector3 hitPoint = ray.GetPoint(distance);

            Vector3 dir = hitPoint - transform.position;

            dir.y = 0f;

            transform.forward = dir.normalized;
        }
    }

    // 맵 범위를 벗어나지 않도록 위치 제한
    void ClampToMap(ref Vector3 pos)
    {
        if (!mapBounds)
            return;

        Bounds b = mapBounds.bounds;

        pos.x = Mathf.Clamp(pos.x, b.min.x + mapPadding, b.max.x - mapPadding);
        pos.z = Mathf.Clamp(pos.z, b.min.z + mapPadding, b.max.z - mapPadding);
    }

    // 주변 좀비 밀집도를 기반으로 이동 가능 여부 판단
    bool CanMove(Vector3 moveDir)
    {
        float radius = 0.3f;
        float height = 1.4f;

        Vector3 pos = transform.position;

        Collider[] hits = Physics.OverlapCapsule(pos + Vector3.up * 0.4f, pos + Vector3.up * (height - 0.2f), radius, LayerMask.GetMask("NormalZombie", "BossZombie"));

        int weightSum = 0;

        foreach (var hit in hits)
        {
            Zombie_Ctrl z = hit.GetComponentInParent<Zombie_Ctrl>();
            if (z == null || z.isDead)
                continue;

            if (z.zomType == ZombieType.Explosion)
                continue;

            // 일반 좀비는 1, 보스는 3으로 가중치 계산
            if (z.zomType == ZombieType.Normal)
                weightSum += 1;
            else if (z.zomType == ZombieType.Boss)
                weightSum += 3;
        }

        return weightSum <= 4;
    }

    // 데미지 처리 (피해 감소율 적용)
    public void HitDamage(int damage)
    {
        if (isDie)
            return;

        float reduction = Mathf.Clamp01(PlayerStats.Inst.DamageReduction);
        float finalDamage = damage * (1f - reduction);

        PlayerStats.Inst.curHp -= Mathf.RoundToInt(finalDamage);

        hpBar.fillAmount = PlayerStats.Inst.curHp / PlayerStats.Inst.MaxHp;

        if (PlayerStats.Inst.curHp <= 0)
        {
            PlayerStats.Inst.curHp = 0;
            UpdateHpUI();
            StartCoroutine(Die());
            return;
        }

        UpdateHpUI();
    }

    // 체력 UI 갱신
    public void UpdateHpUI()
    {
        hpBar.fillAmount = PlayerStats.Inst.curHp / PlayerStats.Inst.MaxHp;

        hpText.text = $"{PlayerStats.Inst.curHp} / {PlayerStats.Inst.MaxHp}";
    }

    // 사망 처리
    IEnumerator Die()
    {
        isDie = true;
        playerAnim.DieAnim();

        yield return new WaitForSeconds(3f);
        GameMgr.Inst.GameEnd();
    }

    // 경험치 획득 및 레벨업 처리
    public void AddExp(int value)
    {
        curExp += value;

        while (curExp >= maxExp)
        {
            curExp -= maxExp;
            LevelUp();
        }

        expBar.fillAmount = curExp / maxExp;
    }

    // 레벨업 처리
    void LevelUp()
    {
        Sound_Mgr.Inst.StopBGM();
        Sound_Mgr.Inst.StopLoopEffect("Flame");
        Sound_Mgr.Inst.PlayEffSound("LevelUp", 0.8f);

        level++;

        LevelUpMgr.Inst.Show();

        // 요구 경험치 증가 규칙
        if (maxExp <= 300)
        {
            maxExp *= 2;
        }
        else
        {
            maxExp += 100;
        }

        GameMgr.Inst.levelText.text = "Lv " + level;
    }

}
