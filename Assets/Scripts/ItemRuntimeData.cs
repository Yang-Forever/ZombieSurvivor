using UnityEngine;

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

    #region 스텟 적용 함수
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
                ApplySubWeapon();
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
        // 최초 획득 시 장착
        if (curLevel == 1 && baseData.itemType == ItemType.MainWeapon)
        {
            Gun.Inst.SetWeapon(this);
        }
    }

    void ApplySubWeapon()
    {
        // 최초 휙득 시 장착
        if (curLevel == 1 && baseData.itemType == ItemType.SubWeapon)
        {
            SubWeaponMgr.Inst.SpawnSubWeapon(baseData, this);
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
    #endregion

    #region 실시간 참조 함수
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

    public float GetInterval()
    {
        float baseInterval = baseData.value1[0];

        float reduceRate = 0f;

        for (int i = 1; i < curLevel && i < baseData.value1.Length; i++)
            reduceRate += baseData.value1[i];

        reduceRate = Mathf.Clamp(reduceRate, 0f, 0.8f);

        float interval = baseInterval * ((1f - reduceRate) / PlayerStats.Inst.AttackSpeed);

        return Mathf.Max(0.1f, interval);
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

    public int GetPelletCount()
    {
        int result = 0;

        if (baseData.value3Type != Value3Type.PelletCount)
            return 0;

        if (baseData.value3 == null)
            return 0;

        for (int i = 0; i < curLevel && i < baseData.value3.Length; i++)
            result += (int)baseData.value3[i];

        return result;
    }

    public float GetCoolTime()
    {
        float result = baseData.value1[0];

        for (int i = 1; i < curLevel; i++)
            result -= baseData.value1[i];

        return result;
    }

    float GetAccumulatedValue(float[] arr, int curLevel)
    {
        if (arr == null)
            return 0f;

        float sum = 0f;

        if (baseData.itemType == ItemType.Passive)
        {
            for (int i = 0; i < curLevel && i < arr.Length; i++)
                sum += arr[i];
        }
        else
        {
            for(int i = 1; i < curLevel && i < arr.Length; i++)
                sum += arr[i];
        }

        return sum;
    }

    public string GetLevelUpDesc(int nextLevel)
    {
        ItemData bd = baseData;

        // 패시브
        if (bd.itemType == ItemType.Passive)
        {
            return ReplaceValue(bd.levelUpDesc, nextLevel);
        }
        // 무기
        else
        {
            if (nextLevel == 1)
                return bd.baseDesc;
            else
                return ReplaceValue(bd.levelUpDesc, nextLevel);
        }
    }

    string ReplaceValue(string temp, int level)
    {
        int idx = level - 1;
        ItemData bd = baseData;
        string result = temp;

        // 조건부 패시브 제거
        if (!IsConditionalActive(PassiveType.Reduction, level))
        {
            result = result.Replace("\n받는 피해 {2}% 감소", "");
        }

        if (!IsConditionalActive(PassiveType.Penetration, level))
        {
            result = result.Replace("\n총알 관통 {2}마리 증가", "");
        }

        return result
            .Replace("{1}", idx < bd.value1.Length ? FormatValue(bd.value1[idx], bd.value1Display) : "")
            .Replace("{2}", idx < bd.value2.Length ? FormatValue(bd.value2[idx], bd.value2Display) : "")
            .Replace("{3}", idx < bd.value3.Length ? FormatValue(bd.value3[idx], bd.value3Display) : "");
    }

    public string GetTooltipDesc()
    {
        if (baseData == null || curLevel <= 0)
            return "";

        string temp = baseData.valueDesc;
        string result = temp;

        float v1 = GetAccumulatedValue(baseData.value1, curLevel);
        float v2 = GetAccumulatedValue(baseData.value2, curLevel);
        float v3 = GetAccumulatedValue(baseData.value3, curLevel);

        if (v2 <= 0f)
        {
            result = result.Replace("\n받는 피해 {2}% 감소", "")
                           .Replace("\n총알 관통 {2}마리 증가", "");

        }

        return result
                .Replace("{0}", baseData.baseDamage.ToString())
                .Replace("{1}", FormatValue(v1, baseData.value1Display))
                .Replace("{2}", FormatValue(v2, baseData.value2Display))
                .Replace("{3}", FormatValue(v3, baseData.value3Display));
    }
    string FormatValue(float value, ValueDisplayType type)
    {
        switch (type)
        {
            case ValueDisplayType.Percent:
                return (value * 100f).ToString("0.#");
            case ValueDisplayType.Raw:
            default:
                return value.ToString("0.#");
        }
    }
    #endregion
}