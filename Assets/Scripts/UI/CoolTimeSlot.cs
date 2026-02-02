using UnityEngine;
using UnityEngine.UI;

public class CoolTimeSlot : MonoBehaviour
{
    public Image iconImg;
    public Image coolTimeBar;

    SubWeaponBase weapon;

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
