using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 레벨업 선택 화면에서 하나의 아이템 카드를 담당하는 UI 버튼
/// 아이템 정보 표시 및 선택 시 아이템 적용을 요청
/// </summary>
public class LevelUpPickBtn : MonoBehaviour
{
    [Header("UI Setting")]
    public Image itemIcon;
    public Text itemType;
    public Text itemName;
    public Text itemLevel;
    public Text itemDesc;
    public Text newText;

    private ItemRuntimeData itemData;   // 버튼이 선택한 아이템

    // 레벨업 카드 UI 세팅
    public void SetUp(ItemRuntimeData data)
    {
        itemData = data;

        itemIcon.sprite = data.baseData.itemIcon;
        itemType.text = data.baseData.itemType != 0 ? "무기" : "패시브";
        itemName.text = data.baseData.itemName;

        // 실제 적용될 다음 레벨 표시
        itemLevel.text = "Lv " + (data.curLevel + 1);

        // 다음 레벨 기준 설명 문구
        itemDesc.text = data.GetLevelUpDesc(data.curLevel + 1);

        // 처음 획득 시 NEW 표시
        newText.gameObject.SetActive(data.curLevel == 0);
    }

    // 카드 선택 시 호출
    public void OnClick()
    {
        Sound_Mgr.Inst.PlayGUISound("UI_Click", 0.4f);
        LevelUpMgr.Inst.SelectItem(itemData);
    }
}
