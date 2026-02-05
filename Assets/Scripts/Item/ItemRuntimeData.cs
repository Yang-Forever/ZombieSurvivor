using UnityEngine;

/// <summary>
/// 아이템 레벨, 소유 여부, 등장 가능 여부 관리
/// 패시브 / 무기 / 서브무기 효과 적용
/// 실시간 스텟 계산 및 UI 설명 텍스트 출력
/// </summary>
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
    // 아이템 종류에 따른 효과 적용
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

    // 패시브 종류에 따른 효과 적용
    void ApplyPassive()
    {
        int level = curLevel;      // 1-based
        int idx = level - 1;

        if (level < 0)
            return;

        // 기본 패시브 스텟 적용
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

        // 조건부 패시브 적용
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
                case PassiveType.Penetration:
                    PlayerStats.Inst.AddPenetration((int)value);
                    break;
            }
        }
    }

    // 특정 레벨에서 조건부 패시브가 활성화되는지 확인
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

    // 메인 무기 적용
    void ApplyWeapon()
    {
        // 최초 획득 시 장착
        if (curLevel == 1 && baseData.itemType == ItemType.MainWeapon)
        {
            Gun.Inst.SetWeapon(this);
        }
    }

    // 서브 무기 적용
    void ApplySubWeapon()
    {
        // 최초 휙득 시 장착
        if (curLevel == 1 && baseData.itemType == ItemType.SubWeapon)
        {
            SubWeaponMgr.Inst.SpawnSubWeapon(baseData, this);
        }
    }

    // 배열 접근용 값 추출
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
    // 레벨 기반 가중치 (등장 확률)
    public float GetWeight()
    {
        if (!canAppear)
            return 0f;

        return baseData.baseWeight + curLevel * baseData.levelWeightIncrease;
    }

    // 누적 데미지 배율 계산
    public float GetDamageRatio()
    {
        float sum = 0f;

        for (int i = 0; i < curLevel; i++)
            sum += baseData.value2[i];

        return 1f + sum;
    }

    // 공격속도 계산 (발사 간격)
    public float GetInterval()
    {
        float baseInterval = baseData.value1[0];

        float reduceRate = 0f;

        for (int i = 1; i < curLevel && i < baseData.value1.Length; i++)
            reduceRate += baseData.value1[i];

        reduceRate = Mathf.Clamp(reduceRate, 0f, 1.0f);

        float interval = baseInterval * ((1f - reduceRate) / PlayerStats.Inst.AttackSpeed);

        return Mathf.Max(0.03f, interval);
    }

    // 샷건 펠릿(분열 총알) 개수 계산
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

    // 화염방사기 길이 계산
    public float GetLength()
    {
        float length = baseData.value3[0];

        for (int i = 1; i < curLevel && i < baseData.value3.Length; i++)
            length += baseData.value3[i];

        return length;
    }

    // 화염방사기 틱 데미지 계산
    public float GetFlameTickDamage()
    {
        return baseData.baseDamage
            * GetDamageRatio()
            * PlayerStats.Inst.DamageMultiplier;
    }

    // 화염방사기 틱 간격 계산
    public float GetFlameTickInterval()
    {
        // 기본 틱 간격
        float baseTick = baseData.value1[0];

        // 무기 레벨 기반 공속(value1)
        float weaponSpeedBonus = 0f;
        for (int i = 1; i < curLevel && i < baseData.value1.Length; i++)
            weaponSpeedBonus += baseData.value1[i];   // 예: 0.05f씩

        // 플레이어 공격속도 (레이저는 영향 약하게)
        float playerAtkSpeed = 1f + PlayerStats.Inst.AttackSpeed * 0.01f;

        // 최종 틱 간격
        float interval =
            baseTick
            / (1f + weaponSpeedBonus)
            / playerAtkSpeed;

        // 안전 클램프
        return Mathf.Clamp(interval, 0.02f, baseTick);
    }

    // 화염방사기 초당 증가량
    public float GetFlameHeatIncrease()
    {
        return 1f;
    }

    // 화염방사기 최대량
    public float GetFlameMaxHeat()
    {
        return 10f;
    }

    // 화염방사기 초당 감소량
    public float GetFlameCoolDown()
    {
        return 2.5f;
    }

    // 쿨타임 감소량 계산
    public float GetCoolTime()
    {
        float result = baseData.value1[0];

        for (int i = 1; i < curLevel; i++)
            result -= baseData.value1[i];

        return result;
    }

    // 배열에 들어있는 값을 레벨 기준으로 누적해서 합산
    // 패시브와 무기는 누적 방식이 다르기 때문에 분기 처리
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

    // 레벨업 시 표시될 설명 텍스트 반환
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

    // 설명에 값 추가
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

    // 현재 보유 중인 아이템의 툴팁 설명 반환
    public string GetTooltipDesc()
    {
        if (baseData == null || curLevel <= 0)
            return "";

        if (baseData.itemType != ItemType.Passive && curLevel == 1)
        {
            return baseData.baseDesc;
        }

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

    // 수치 표시 형식 변환 (퍼센트 / 일반 수치)
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