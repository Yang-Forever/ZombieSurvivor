using UnityEngine;
using UnityEngine.UI;

public class LevelUpPickBtn : MonoBehaviour
{
    [Header("UI Setting")]
    public Image itemIcon;
    public Text itemType;
    public Text itemName;
    public Text itemLevel;
    public Text itemDesc;
    public Text newText;

    private ItemRuntimeData itemData;

    public void SetUp(ItemRuntimeData data)
    {
        itemData = data;

        itemIcon.sprite = data.baseData.itemIcon;
        itemType.text = data.baseData.itemType != 0 ? "무기" : "패시브";
        itemName.text = data.baseData.itemName;
        itemLevel.text = "Lv " + (data.curLevel + 1);
        itemDesc.text = data.GetLevelUpDesc(data.curLevel + 1);

        newText.gameObject.SetActive(data.curLevel == 0);
    }

    public void OnClick()
    {
        LevelUpMgr.Inst.SelectItem(itemData);
    }
}
