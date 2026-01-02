using Unity.VisualScripting.Antlr3.Runtime;

[System.Serializable]
public class ItemRuntimeData
{
    public ItemData baseData;
    public int curLevel;
    public bool isOwned;
    public bool canAppear;

    public ItemRuntimeData(ItemData data)
    {
        baseData = data;
        curLevel = 0;
        isOwned = false;
        canAppear = true;
    }

    public void Apply()
    {
        switch (baseData.itemType)
        {
            case ItemType.Passive:
                ApplyPassive();
                break;

            case ItemType.MainWeapon:
                ApplyWeapon();
                break;

            case ItemType.SubWeapon:
                break;
        }
    }

    void ApplyPassive()
    {
        int level = curLevel;      // 1-based
        int idx = level - 1;

        if (level < 0)
            return;

        for (int i = 0; i < baseData.passiveType.Length; i++)
        {
            PassiveType type = baseData.passiveType[i];
            float value = GetValue(baseData.value1, idx);

            switch (type)
            {
                case PassiveType.AtkSpeed:
                    PlayerStats.Inst.AddAtkSpeed(value);
                    break;

                case PassiveType.AtkDamage:
                    PlayerStats.Inst.AddDmgMultyplier(value);
                    break;

                case PassiveType.MoveSpeed:
                    PlayerStats.Inst.AddMvSpeed(value);
                    break;

                case PassiveType.HpMax:
                    PlayerStats.Inst.AddHp(value);
                    break;

                case PassiveType.MagnetRange:
                    PlayerStats.Inst.AddMagnetRange(value);
                    break;

                case PassiveType.Reduction:
                    PlayerStats.Inst.AddReduction(value);
                    break;

                case PassiveType.ReloadSpeed:
                    PlayerStats.Inst.AddReloadSpeed(value);
                    break;

                case PassiveType.Magazine:
                    PlayerStats.Inst.AddMagazine((int)value);
                    break;

                case PassiveType.Penetration:
                    PlayerStats.Inst.AddPenetration((int)value);
                    break;

                default:
                    break;
            }
        }

        if (baseData.conditionalPassives == null)
            return;

        foreach (var cp in baseData.conditionalPassives)
        {
            if (!IsConditionalActive(cp.type, level))
                continue;

            float value = GetValue(baseData.value2, idx);

            switch (cp.type)
            {
                case PassiveType.Reduction:
                    PlayerStats.Inst.AddReduction(value);
                    break;

                case PassiveType.ReloadSpeed:
                    PlayerStats.Inst.AddReloadSpeed(value);
                    break;
            }
        }
    }

    bool IsConditionalActive(PassiveType type, int level)
    {
        if (baseData.conditionalPassives == null)
            return false;

        foreach (var cp in baseData.conditionalPassives)
        {
            if (cp.type != type)
                continue;

            foreach (int lv in cp.activeLevels)
            {
                if (lv == level)
                    return true;
            }
        }
        return false;
    }

    void ApplyWeapon()
    {
        // ÃÖÃÊ È¹µæ ½Ã ÀåÂø
        if (curLevel == 1 && baseData.itemType == ItemType.MainWeapon)
        {
            Gun.Inst.SetWeapon(this);
        }
    }

    float GetValue(float[] arr, int idx)
    {
        if (arr == null)
            return 0f;

        if (idx < 0 || idx >= arr.Length)
            return 0f;

        return arr[idx];
    }


    public float GetWeight()
    {
        if (!canAppear)
            return 0f;

        return baseData.baseWeight + curLevel * baseData.levelWeightIncrease;
    }

    public float GetDamageRatio()
    {
        float sum = 0f;

        for (int i = 0; i < curLevel; i++)
            sum += baseData.value2[i];

        return 1f + sum;
    }

    public int GetPenetration()
    {
        int result = baseData.penetration;

        if (baseData.value3Type != Value3Type.PenetrationCount)
            return result;

        for (int i = 0; i < curLevel && i < baseData.value3.Length; i++)
            result += (int)baseData.value3[i];

        return result;
    }
}