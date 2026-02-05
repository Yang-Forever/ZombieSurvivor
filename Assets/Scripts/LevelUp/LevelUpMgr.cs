using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 레벨업 시 아이템 선택 UI 및 아이템 성장 로직 관리
/// </summary>
public class LevelUpMgr : MonoBehaviour
{
    [Header("UI Setting")]
    public GameObject lvPanel;
    public LevelUpPickBtn[] lvPickBtn;

    public List<ItemData> items;        // 전체 아이템 원본 데이터
    List<ItemRuntimeData> runtimeItems;

    bool firstLvUp = true;  // 첫 레벨업 여부 

    public static LevelUpMgr Inst = null;

    private void Awake()
    {
        Inst = this;

        InitRuntimeItems(); // 아이템 런타임 데이터 생성
    }

    // ItemData → ItemRuntimeData 변환
    public void InitRuntimeItems()
    {
        runtimeItems = new List<ItemRuntimeData>();

        foreach (var item in items)
            runtimeItems.Add(new ItemRuntimeData(item));
    }

    // 레벨업 UI 표시
    public void Show()
    {
        GameMgr.Inst.ChangeState(GameState.LevelUp);
        GameMgr.Inst.timeText.gameObject.SetActive(false);
        lvPanel.SetActive(true);

        // 선택 가능한 아이템 3개 추출
        List<ItemRuntimeData> canPickItems = GetRandomItem(3);

        for (int i = 0; i < lvPickBtn.Length; i++)
        {
            if (i < canPickItems.Count)
            {
                lvPickBtn[i].SetUp(canPickItems[i]);
                lvPickBtn[i].gameObject.SetActive(true);
            }
            else
            {
                lvPickBtn[i].gameObject.SetActive(false);
                Hide();
            }
        }
    }

    // 레벨업 UI 종료
    public void Hide()
    {
        GameMgr.Inst.timeText.gameObject.SetActive(true);
        lvPanel.SetActive(false);
        GameMgr.Inst.ChangeState(GameState.Play);
        Sound_Mgr.Inst.ResumeBGM();
    }

    // 등장 가능한 아이템 중 랜덤 선택
    List<ItemRuntimeData> GetRandomItem(int count)
    {
        List<ItemRuntimeData> getItem = new List<ItemRuntimeData>();

        // 첫 레벨업: 시작 무기만 등장
        if (firstLvUp)
        {
            foreach (var item in runtimeItems)
            {
                if (item.baseData.isStartOnly)
                    continue;

                if (item.baseData.itemType == ItemType.MainWeapon)
                    getItem.Add(item);
            }
            
            firstLvUp = false;
        }
        else
        {
            foreach(var item in runtimeItems)
            {
                if (!item.canAppear)
                    continue;

                if (item.curLevel >= item.baseData.maxLevel)
                    continue;

                if (item.baseData.isStartOnly)
                    continue;

                // 메인 무기는 이미 소유한 경우만 등장
                if (!(item.baseData.itemType == ItemType.MainWeapon) || item.isOwned)
                    getItem.Add(item);
            }
        }

        // 가중치 랜덤 선택
        List<ItemRuntimeData> picked = GetWeightRandomItem(getItem, count);

        // 아이템 부족 시 카드 중복 출현
        if (picked.Count > 0 && picked.Count < count)
        {
            while (picked.Count < count)
            {
                int rand = Random.Range(0, picked.Count);
                picked.Add(picked[rand]);
            }
        }

        return picked;
    }

    // 가중치 기반 랜덤 선택
    List<ItemRuntimeData> GetWeightRandomItem(List<ItemRuntimeData> list, int count)
    {
        List<ItemRuntimeData> result = new List<ItemRuntimeData>();
        List<ItemRuntimeData> getItem = new List<ItemRuntimeData>(list);

        for(int i = 0; i < count; i++)
        {
            if (getItem.Count == 0)
                break;

            float totalWeight = 0.0f;
            foreach(var item in getItem)
                totalWeight += item.GetWeight();

            float randomvalue = Random.Range(0, totalWeight);

            float curWeight = 0.0f;
            ItemRuntimeData selected = null;

            foreach(var item in getItem)
            {
                curWeight += item.GetWeight();
                if(curWeight >= randomvalue)
                {
                    selected = item;
                    break;
                }
            }

            result.Add(selected);
            getItem.Remove(selected);
        }

        return result;
    }

    // 아이템 선택 처리
    public void SelectItem(ItemRuntimeData item)
    {
        item.isOwned = true;
        item.curLevel++;

        // 최대 레벨 도달 시 등장 제한
        if (item.curLevel >= item.baseData.maxLevel)
            item.canAppear = false;

        item.Apply();   // 스텟 / 무기 적용

        Hide();
    }

    // 특정 무기 타입 런타임 데이터 검색
    public ItemRuntimeData FindRuntimeWeapon(MainWeaponType weaponType)
    {
        foreach (var item in runtimeItems)
        {
            if (item.baseData.mainWeapon == weaponType)
                return item;
        }

        return null;
    }

    // 소유 중인 아이템 목록 반환
    public List<ItemRuntimeData> GetOwnedItems()
    {
        List<ItemRuntimeData> owned = new List<ItemRuntimeData>();

        foreach (var item in runtimeItems)
        {
            if (item.isOwned && item.curLevel > 0)
                owned.Add(item);
        }

        return owned;
    }
}
