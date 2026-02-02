using UnityEngine;

public class SubWeaponGrenade : SubWeaponBase
{
    public GameObject grenadePrefab;
    public Transform firePos;
    public float power = 10f;

    public override void Use()
    {
        GameObject go = Instantiate(grenadePrefab, firePos.transform.position, Quaternion.identity);
    }

    
}
