using UnityEngine;

/// <summary>
/// 서브 무기 쿨타임 UI 패널
/// 보유한 서브 무기의 쿨타임 슬롯을 추가하여 표시하는 역할
/// </summary>
public class ItemCoolTimePanel : MonoBehaviour
{
    public Transform content;
    public CoolTimeSlot slotPrefab;

    // 서브 무기 쿨타임 슬룻 추가 + 서브 무기 휙득 시 호출
    public void AddSlot(SubWeaponBase weapon)
    {
        CoolTimeSlot slot = Instantiate(slotPrefab, content);

        // 슬롯에 서브 무기 데이터 연결
        slot.Init(weapon);
    }
}
