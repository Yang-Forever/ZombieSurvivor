using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 인벤토리 / UI 슬룻 담당
/// 아이템 아이콘 표시 + 마우스 오버 시 툴팁 출력
/// </summary>
public class ItemSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Image iconImg;
    public Text levelText;

    ItemRuntimeData data;

    // 슬롯에 표시할 아이템 데이터 초기화
    public void Init(ItemRuntimeData runtimeData)
    {
        data = runtimeData;

        iconImg.sprite = data.baseData.itemIcon;
        levelText.text = $"Lv {data.curLevel}";
    }

    // 마우스가 슬롯 위에 올라갔을 때 툴팁 표시
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (data == null)
            return;

        // 마우스 위치 기준으로 툴팁 표시
        TooltipBox.Inst.Show(data, eventData.position);
    }

    // 마우스가 슬롯에서 벗어났을 때 툴팁 숨김
    public void OnPointerExit(PointerEventData eventData)
    {
        TooltipBox.Inst.Hide();
    }
}
