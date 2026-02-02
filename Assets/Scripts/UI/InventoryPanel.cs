using System.Collections.Generic;
using UnityEngine;

public class InventoryPanel : MonoBehaviour
{
    public Transform content;
    public ItemSlot slotPrefab;

    List<ItemSlot> slots = new List<ItemSlot>();

    private void OnEnable()
    {
        RefreshUI();
    }

    public void RefreshUI()
    {
        foreach (var slot in slots)
            Destroy(slot.gameObject);

        slots.Clear();

        List<ItemRuntimeData> items = LevelUpMgr.Inst.GetOwnedItems();

        foreach(var item in items)
        {
            ItemSlot slot = Instantiate(slotPrefab, content);
            slot.Init(item);
            slots.Add(slot);
        }
    }
}
