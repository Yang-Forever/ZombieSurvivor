using UnityEngine;

/// <summary>
/// 서브 무기 생성 및 쿨타임 UI 등록을 관리
/// </summary>
public class SubWeaponMgr : MonoBehaviour
{
    public ItemCoolTimePanel coolTimePanel;

    public static SubWeaponMgr Inst = null;

    private void Awake()
    {
        Inst = this;
    }

    // 서브 무기를 생성하고 쿨타임 UI에 등록
    public void SpawnSubWeapon(ItemData itemData, ItemRuntimeData runtimeData)
    {
        GameObject go = Instantiate(itemData.subWeaponPrefab, transform);
        SubWeaponBase weapon = go.GetComponent<SubWeaponBase>();
        weapon.Init(runtimeData);

        coolTimePanel.AddSlot(weapon);
    }
}
