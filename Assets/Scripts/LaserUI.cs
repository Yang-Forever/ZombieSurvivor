using UnityEngine;
using UnityEngine.UI;

public class LaserUI : MonoBehaviour
{
    [Header("UI")]
    public Image heatFill;

    [Header("Color")]
    public Color normalColor = new Color32(255, 255, 255, 180);
    public Color halfColor = new Color32(255, 127, 0, 180);
    public Color overHeatColor = new Color32(255, 0, 0, 180);

    Laser_Ctrl laser;

    void Start()
    {
        laser = FindObjectOfType<Laser_Ctrl>();
    }

    void Update()
    {
        if (laser == null || heatFill == null)
            return;

        float ratio = laser.GetHeatRatio();
        heatFill.fillAmount = ratio;

        if (laser.IsOverHeat())
        {
            heatFill.color = overHeatColor;
        }
        else if (ratio >= 0.5f)
        {
            heatFill.color = halfColor;
        }
        else
        {
            heatFill.color = normalColor;
        }
    }

    public void SetVisible(bool isOn)
    {
        if (gameObject.activeSelf != isOn)
            gameObject.SetActive(isOn);
    }
}
