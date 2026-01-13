using UnityEngine;
using UnityEngine.UI;

public class OrbitalStrikeSkill : MonoBehaviour
{
    [Header("Setting")]
    public GameObject projectilePrefab;
    public Image coolTimeImg;
    float coolTime = 2f;
    float timer = 2;

    public float distance = 10f;
    public Transform player;

    public static OrbitalStrikeSkill Inst = null;

    private void Awake()
    {
        Inst = this;
    }

    // Update is called once per frame
    void Update()
    {
        if(timer <= 0)
        {
            timer = 0;
            if(Input.GetKeyDown(KeyCode.Q))
            {
                UseOrbitalStrike();
                timer = coolTime;
            }
        }
        else
        {
            timer -= Time.deltaTime;
        }

        coolTimeImg.fillAmount = timer / coolTime;
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
}
