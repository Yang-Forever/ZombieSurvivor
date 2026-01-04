using UnityEngine;

public class SubWeaponMgr : MonoBehaviour
{
    public ItemCoolTimePanel coolTimePanel;

    public static SubWeaponMgr Inst = null;

    private void Awake()
    {
        Inst = this;
    }

    public void SpawnSubWeapon(ItemData itemData, ItemRuntimeData runtimeData)
    {
        GameObject go = Instantiate(itemData.subWeaponPrefab, transform);
        SubWeaponBase weapon = go.GetComponent<SubWeaponBase>();
        weapon.Init(runtimeData);

        coolTimePanel.AddSlot(weapon);
    }
}
