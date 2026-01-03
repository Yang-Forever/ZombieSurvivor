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
        itemDesc.text = GetLevelDesc(data, data.curLevel + 1);

        newText.gameObject.SetActive(data.curLevel == 0);
    }

    string GetLevelDesc(ItemRuntimeData data, int nextLevel)
    {
        ItemData bd = data.baseData;

        // 패시브
        if (bd.itemType == ItemType.Passive)
        {
            return ReplaceValue(bd.levelUpDesc, bd, nextLevel);
        }
        // 무기
        else
        {
            if (nextLevel == 1)
                return ReplaceValue(bd.baseDesc, bd, nextLevel);
            else
                return ReplaceValue(bd.levelUpDesc, bd, nextLevel);
        }
    }

    string ReplaceValue(string temp, ItemData data, int nextLevel)
    {
        int idx = nextLevel - 1;
        string result = temp;

        // Reduction
        if (!IsConditionalActive(data, PassiveType.Reduction, nextLevel))
        {
            result = result.Replace("\n받는 피해 {2}% 감소", "");
        }

        if (!IsConditionalActive(data, PassiveType.ReloadSpeed, nextLevel))
        {
            //result = result.Replace("\n재장전 속도 감소 {2}%", "");
        }

        return result
            .Replace("{1}", idx < data.value1.Length ? data.value1[idx].ToString() : "")
            .Replace("{2}", idx < data.value2.Length ? data.value2[idx].ToString() : "")
            .Replace("{3}", idx < data.value3.Length ? data.value3[idx].ToString() : "");
    }

    bool IsConditionalActive(ItemData data, PassiveType type, int level)
    {
        if (data.conditionalPassives == null)
            return false;

        foreach (var cp in data.conditionalPassives)
        {
            if (cp.type != type)
                continue;

            foreach (int lv in cp.activeLevels)
            {
                if (lv == level)
                    return true;
            }
        }
        return false;
    }


    public void OnClick()
    {
        LevelUpMgr.Inst.SelectItem(itemData);
    }
}
