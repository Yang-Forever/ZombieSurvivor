using UnityEngine;
using UnityEngine.UI;

public class ItemSlot : MonoBehaviour
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
}
