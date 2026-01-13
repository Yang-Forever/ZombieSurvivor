using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Player_Ctrl : MonoBehaviour
{
    [Header("Player Move")]
    float h = 0.0f;
    float v = 0.0f;
    float rotSpeed = 25.0f;

    [Header("Player Setting")]
    public Image expBar;
    float curExp = 0;
    float maxExp = 50;
    int level = 1;
    public Image hpBar;
    public Text hpText;

    PlayerAnim_Ctrl playerAnim;

    private void Awake()
    {
        playerAnim = GetComponentInChildren<PlayerAnim_Ctrl>();
    }

    // Update is called once per frame
    void Update()
    {
        if (GameMgr.Inst.state != PlayerState.Play)
            return;

        MoveKB();

        // 수동회전
        RotateMouse();

        if (Input.GetKeyDown(KeyCode.Alpha2))
            LevelUp();

        hpText.text = PlayerStats.Inst.curHp + " / " + PlayerStats.Inst.MaxHp;
    }

    void MoveKB()
    {
        // 플레이어 이동
        h = Input.GetAxis("Horizontal");
        v = Input.GetAxis("Vertical");

        Vector3 moveDir = new Vector3(h, 0.0f, v);
        if (moveDir.magnitude > 1.0f)
            moveDir.Normalize();

        transform.position += moveDir * PlayerStats.Inst.MoveSpeed * Time.deltaTime;

        Vector3 localMoveDir = transform.InverseTransformDirection(moveDir);
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

    public void HitDamage(int damage)
    {
        if (PlayerStats.Inst.curHp <= 0)
            return;

        float reduction = PlayerStats.Inst.DamageReduction;
        reduction = Mathf.Clamp01(reduction);

        float finalDamage = damage * (1f - reduction);

        PlayerStats.Inst.curHp -= Mathf.RoundToInt(finalDamage);

        hpBar.fillAmount = PlayerStats.Inst.curHp / PlayerStats.Inst.MaxHp;

        if (PlayerStats.Inst.curHp <= 0)
        {
            PlayerStats.Inst.curHp = 0;

            playerAnim.DieAnim();

            StartCoroutine(Die());
        }
    }
    IEnumerator Die()
    {
        yield return new WaitForSeconds(3f);
        GameMgr.Inst.GameEnd();
    }

    public void AddExp(int value)
    {
        curExp += value;

        expBar.fillAmount = curExp / maxExp;

        if(curExp >= maxExp)
        {
            LevelUp();
        }
    }

    void LevelUp()
    {
        curExp -= maxExp;
        level++;
        curExp = 0;
        LevelUpMgr.Inst.Show();

        if(maxExp <= 300)
        {
            maxExp *= 2;
        }
        else
        {
            maxExp += 100;
        }

        expBar.fillAmount = curExp / maxExp;
        GameMgr.Inst.levelText.text = "Lv " + level;
    }

}
