using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 궤도 폭격 스킬의 충전, 사용, UI 표시를 관리
/// </summary>
public class OrbitalStrikeSkill : MonoBehaviour
{
    [Header("Setting")]
    public GameObject projectilePrefab;
    public Image iconImg;
    public Text countText;

    public int maxCharge = 3;
    int curCharge = 0;

    public float distance = 10f;
    public Transform player;

    public static OrbitalStrikeSkill Inst = null;

    private void Awake()
    {
        Inst = this;

        UpdateUI();
    }

    // 입력 처리 및 스킬 사용
    void Update()
    {
        if (curCharge <= 0)
            return;

        if (Input.GetKeyDown(KeyCode.Q))
        {
            UseOrbitalStrike();
            curCharge--;
            UpdateUI();
        }
    }

    // 궤도 폭격 스킬 사용
    void UseOrbitalStrike()
    {
        if (!GetMousePos(out Vector3 targetPos))
            return;

        Vector3 spawnPos = targetPos + Vector3.up * 30f;

        Instantiate(projectilePrefab, spawnPos, projectilePrefab.transform.rotation).GetComponent<OrbitalProjectile_Ctrl>().SetTarget(targetPos);
    }

    // 마우스 위치를 월드 좌표로 변환
    bool GetMousePos(out Vector3 pos)
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Plane plane = new Plane(Vector3.up, Vector3.zero);

        if (plane.Raycast(ray, out float dist))
        {
            pos = ray.GetPoint(dist);
            return true;
        }

        pos = Vector3.zero;
        return false;
    }

    // 궤도 폭격 충전 수 증가
    public void AddCharge(int amount = 1)
    {
        curCharge = Mathf.Clamp(curCharge + amount, 0, maxCharge);
        UpdateUI();
    }

    // 스킬 아이콘 및 충전 UI 갱신
    void UpdateUI()
    {
        if (curCharge <= 0)
        {
            iconImg.gameObject.SetActive(false);
            countText.gameObject.SetActive(false);
        }
        else
        {
            iconImg.gameObject.SetActive(true);
            countText.text = curCharge.ToString();
            countText.gameObject.SetActive(true);
        }
    }
}
