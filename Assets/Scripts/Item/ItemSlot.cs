using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ItemSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Image iconImg;
    public Text levelText;

    ItemRuntimeData data;

    public void Init(ItemRuntimeData runtimeData)
    {
        data = runtimeData;

        iconImg.sprite = data.baseData.itemIcon;
        levelText.text = $"Lv {data.curLevel}";
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (data == null)
            return;

        TooltipBox.Inst.Show(data, eventData.position);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        TooltipBox.Inst.Hide();
    }
}
