using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 아이템 슬롯에 마우스를 올렸을 때 설명 텍스트를 표시/숨김 처리한다
/// </summary>
public class TooltipBox : MonoBehaviour
{
    public GameObject tooltipBox;
    public Text descText;

    public static TooltipBox Inst = null;

    private void Awake()
    {
        Inst = this;
        Hide();
    }

    // 툴팁 표시 (아이템 데이터 + 마우스 위치 기준)
    public void Show(ItemRuntimeData data, Vector3 mousePos)
    {
        // 아이템 설명 텍스트 갱신
        descText.text = data.GetTooltipDesc();

        // 마우스 커서와 겹치지 않도록 약간의 오프셋 적용
        Vector3 offset = new Vector3(10f, 10f, 0);

        tooltipBox.SetActive(true);
        tooltipBox.transform.position = mousePos + offset;
    }

    // 툴팁 숨김
    public void Hide()
    {
        tooltipBox.SetActive(false);
    }
}
