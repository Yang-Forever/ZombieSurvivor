using UnityEngine;

public class ItemCoolTimePanel : MonoBehaviour
{
    public Transform content;
    public CoolTimeSlot slotPrefab;

    public void AddSlot(SubWeaponBase weapon)
    {
        CoolTimeSlot slot = Instantiate(slotPrefab, content);
        slot.Init(weapon);
    }
}
