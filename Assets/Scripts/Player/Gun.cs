using UnityEngine;

public class Gun : MonoBehaviour
{
    [HideInInspector]
    public ItemRuntimeData curWeapon;

    public static Gun Inst = null;

    private void Awake()
    {
        Inst = this;
    }

    public void SetWeapon(ItemRuntimeData weapon)
    {
        curWeapon = weapon;
    }
}
