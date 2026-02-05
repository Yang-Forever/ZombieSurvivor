using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 서브 무기 쿨타임 UI 슬룻
/// 서브 무기의 현재 쿨타임 상태를 게이지로 표시
/// </summary>
public class CoolTimeSlot : MonoBehaviour
{
    public Image iconImg;
    public Image coolTimeBar;

    SubWeaponBase weapon;   // 연결된 서브 무기

    // 슬롯 초기화
    public void Init(SubWeaponBase subWeapon)
    {
        weapon = subWeapon;
        iconImg.sprite = weapon.Data.baseData.itemIcon;
    }

    void Update()
    {
        if (weapon == null) return;

        coolTimeBar.fillAmount = weapon.GetCooldownRatio();
    }
}
