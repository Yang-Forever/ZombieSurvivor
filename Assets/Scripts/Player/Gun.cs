using UnityEngine;

/// <summary>
/// 현재 플레이어가 장착 중인 메인 무기를 관리
/// 다른 시스템에서 공통으로 무기 정보를 참조하기 위한 중계 역할
/// </summary>
public class Gun : MonoBehaviour
{
    [HideInInspector]
    public ItemRuntimeData curWeapon;   // 현재 장착 무기

    public static Gun Inst = null;

    private void Awake()
    {
        Inst = this;
    }

    // 무기 교체 시 호출
    public void SetWeapon(ItemRuntimeData weapon)
    {
        curWeapon = weapon;
    }
}
