using UnityEngine;
using UnityEngine.UI;

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

    // Update is called once per frame
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

    void UseOrbitalStrike()
    {
        if (!GetMousePos(out Vector3 targetPos))
            return;

        Vector3 spawnPos = targetPos + Vector3.up * 30f;

        Instantiate(projectilePrefab, spawnPos, projectilePrefab.transform.rotation).GetComponent<OrbitalProjectile_Ctrl>().SetTarget(targetPos);
    }

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

    public void AddCharge(int amount = 1)
    {
        curCharge = Mathf.Clamp(curCharge + amount, 0, maxCharge);
        UpdateUI();
    }

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
