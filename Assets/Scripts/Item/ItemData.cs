using UnityEngine;

// 아이템 종류
public enum ItemType
{
   Passive, MainWeapon, SubWeapon
}

// 메인 무기 종류
public enum MainWeaponType
{
    None, Pistol, Rifle, Shotgun, Flamethrower
}

// 패시브 종류
public enum PassiveType
{
    None, AtkSpeed, AtkDamage, MoveSpeed, HpMax, MagnetRange, Reduction, Penetration
}

// 밸류3번 종류
public enum Value3Type
{
    None, PelletCount, FlameTickLength, ExplosionRange, KnifeRange
}

// 밸류 표시값 종류
public enum ValueDisplayType
{
    Raw,        // 그대로 표시
    Percent     // 퍼센트 표시
}

// 특정 조건(레벨 별로 적용되는 패시브) 패시브
[System.Serializable]
public class ConditionalPassive
{
    public PassiveType type;
    public int[] activeLevels;
}


[CreateAssetMenu(fileName = "Item", menuName = "SO/Item")]
public class ItemData : ScriptableObject
{
    public bool isStartOnly = false;

    [Header("Main Info")]
    public ItemType itemType;
    public MainWeaponType mainWeapon;
    public PassiveType[] passiveType;   // 항상 적용
    public string itemName;
    public Sprite itemIcon;

    [Header("Conditional Passive")]
    public ConditionalPassive[] conditionalPassives;    // 조건 적용

    [Header("Item Level")]
    public int maxLevel = 5;

    [Header("Weight (LevelUp Pick)")]
    public float baseWeight = 1f;
    public float levelWeightIncrease = 0.3f;

    [Header("Weapon Setting")]
    public float baseDamage;
    public int penetration;
    [TextArea]
    public string baseDesc;

    [Header("Sub Weapon Setting")]
    public GameObject subWeaponPrefab;

    [Header("Level Value")]
    public float[] value1;
    public float[] value2;
    public Value3Type value3Type;
    public float[] value3;
    public ValueDisplayType value1Display = ValueDisplayType.Raw;
    public ValueDisplayType value2Display = ValueDisplayType.Raw;
    public ValueDisplayType value3Display = ValueDisplayType.Raw;
    [TextArea] public string levelUpDesc;

    [TextArea]  // 테스트용
    public string valueDesc;
}

