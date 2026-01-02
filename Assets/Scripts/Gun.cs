using UnityEngine;

public class Gun : MonoBehaviour
{
    [Header("Runtime")]
    public int curMagazine;
    public bool isReload;

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
        curMagazine = GetMaxMagazine();
    }

    public int GetMaxMagazine()
    {
        int baseMagazine = curWeapon.baseData.baseMagazine;
        int bonus = PlayerStats.Inst.bonusMagazine;
        return baseMagazine + bonus;
    }

    #region Event Function

    public void EventReload()
    {
        isReload = false;
        curMagazine = GetMaxMagazine();
    }

    #endregion

}
