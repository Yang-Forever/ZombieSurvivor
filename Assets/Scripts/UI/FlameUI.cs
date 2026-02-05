using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 화염방사기 전용 과열 UI
/// 현재 열기 비율에 따라 게이지와 색상을 변경
/// </summary>
public class FlameUI : MonoBehaviour
{
    [Header("UI")]
    public Image heatFill;

    [Header("Color")]
    public Color normalColor = new Color32(255, 255, 255, 180);
    public Color halfColor = new Color32(255, 127, 0, 180);
    public Color overHeatColor = new Color32(255, 0, 0, 180);

    Flame_Ctrl flame;

    void Start()
    {
        flame = FindObjectOfType<Flame_Ctrl>();
    }

    void Update()
    {
        if (flame == null || heatFill == null)
            return;

        // 현재 열기 비율 (0~1)
        float ratio = flame.GetHeatRatio();
        heatFill.fillAmount = ratio;

        if (flame.IsOverHeat())
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

    // 화염 무기 사용 여부에 따라 UI 표시/숨김
    public void SetVisible(bool isOn)
    {
        if (gameObject.activeSelf != isOn)
            gameObject.SetActive(isOn);
    }
}
