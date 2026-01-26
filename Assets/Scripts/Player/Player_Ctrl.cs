using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Player_Ctrl : MonoBehaviour
{
    [Header("Player Move")]
    float h = 0.0f;
    float v = 0.0f;
    float moveBlockRatio = 1f;
    float moveCheckTimer = 0f;
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

    void FixedUpdate()
    {
        if (GameMgr.Inst.state != PlayerState.Play || isDie || !wantMove)
            return;

        float speed = PlayerStats.Inst.MoveSpeed * moveBlockRatio;

        Vector3 nextPos = transform.position + moveDir * speed * Time.fixedDeltaTime;

        ClampToMap(ref nextPos);

        transform.position = nextPos;
    }

    // Update is called once per frame
    void Update()
    {
        if (GameMgr.Inst.state != PlayerState.Play || isDie)
            return;

        MoveKB();          // 이동 판단만
        RotateMouse();     // 회전은 Update OK

        if (Input.GetKeyDown(KeyCode.Alpha2))
            LevelUp();
    }

    void MoveKB()
    {
        h = Input.GetAxisRaw("Horizontal");
        v = Input.GetAxisRaw("Vertical");

        Vector3 input = new Vector3(h, 0f, v);

        if (input.sqrMagnitude < 0.01f)
        {
            wantMove = false;
            playerAnim.MoveAnim(0, 0);
            return;
        }

        input.Normalize();

        moveCheckTimer += Time.deltaTime;
        if (moveCheckTimer >= 0.1f)
        {
            moveCheckTimer = 0f;
            moveBlockRatio = CanMove(input) ? 1f : 0.5f;
        }

        moveDir = input;
        wantMove = true;

        Vector3 localMoveDir = transform.InverseTransformDirection(input);
        playerAnim.MoveAnim(localMoveDir.x, localMoveDir.z);
    }

    void RotateMouse()
    {
        // 플레이어 회전 (컴퓨터용 마우스 회전)
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        // 바닥(y=0) 평면
        Plane plane = new Plane(Vector3.up, Vector3.zero);

        if (plane.Raycast(ray, out float distance))
        {
            Vector3 hitPoint = ray.GetPoint(distance);

            // 플레이어 -> 마우스 위치 방향
            Vector3 dir = hitPoint - transform.position;

            // 탑뷰니까 y는 무시 (수평 방향만)
            dir.y = 0f;

            // 완전 정확한 회전
            transform.forward = dir.normalized;
        }
    }

    void ClampToMap(ref Vector3 pos)
    {
        if (!mapBounds)
            return;

        Bounds b = mapBounds.bounds;

        pos.x = Mathf.Clamp(pos.x, b.min.x + mapPadding, b.max.x - mapPadding);
        pos.z = Mathf.Clamp(pos.z, b.min.z + mapPadding, b.max.z - mapPadding);
    }

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

            if (z.zomType == ZombieType.Normal)
                weightSum += 1;
            else if (z.zomType == ZombieType.Boss)
                weightSum += 3;
        }

        return weightSum <= 4;
    }

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

    public void UpdateHpUI()
    {
        hpBar.fillAmount = PlayerStats.Inst.curHp / PlayerStats.Inst.MaxHp;

        hpText.text = $"{PlayerStats.Inst.curHp} / {PlayerStats.Inst.MaxHp}";
    }

    IEnumerator Die()
    {
        isDie = true;
        playerAnim.DieAnim();

        yield return new WaitForSeconds(3f);
        GameMgr.Inst.GameEnd();
    }

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

    void LevelUp()
    {
        Sound_Mgr.Inst.StopBGM();
        Sound_Mgr.Inst.PlayEffSound("LevelUp", 0.8f);

        level++;

        LevelUpMgr.Inst.Show();

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
