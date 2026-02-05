using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 인벤토리 패널 UI
/// 현재 보유 중인 아이템(ItemRuntimeData)을 슬롯 형태로 표시
/// </summary>
public class InventoryPanel : MonoBehaviour
{
    public Transform content;
    public ItemSlot slotPrefab;

    List<ItemSlot> slots = new List<ItemSlot>();  // 현재 생성된 슬롯 목록

    private void OnEnable()
    {
        // 패널이 열릴 때마다 UI 갱신
        RefreshUI();
    }

    public void RefreshUI()
    {
        // 기존 슬롯 제거
        foreach (var slot in slots)
            Destroy(slot.gameObject);

        slots.Clear();

        // 현재 보유 중인 아이템 목록 가져오기
        List<ItemRuntimeData> items = LevelUpMgr.Inst.GetOwnedItems();

        // 아이템 수만큼 슬롯 생성
        foreach (var item in items)
        {
            ItemSlot slot = Instantiate(slotPrefab, content);
            slot.Init(item);
            slots.Add(slot);
        }
    }
}
