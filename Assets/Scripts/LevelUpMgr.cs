using System.Collections.Generic;
using UnityEngine;

public class LevelUpMgr : MonoBehaviour
{
    [Header("UI Setting")]
    public GameObject lvPanel;
    public LevelUpPickBtn[] lvPickBtn;

    public List<ItemData> items;
    List<ItemRuntimeData> runtimeItems;

    bool firstLvUp = true;

    public static LevelUpMgr Inst = null;

    private void Awake()
    {
        Inst = this;

        InitRuntimeItems();
    }

    public void InitRuntimeItems()
    {
        runtimeItems = new List<ItemRuntimeData>();

        foreach (var item in items)
            runtimeItems.Add(new ItemRuntimeData(item));
    }

    public void Show()
    {
        GameMgr.Inst.ChangeState(PlayerState.LevelUp);
        lvPanel.SetActive(true);

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

    public void Hide()
    {
        lvPanel.SetActive(false);
        GameMgr.Inst.ChangeState(PlayerState.Play);
    }

    List<ItemRuntimeData> GetRandomItem(int count)
    {
        List<ItemRuntimeData> getItem = new List<ItemRuntimeData>();

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

                if (!(item.baseData.itemType == ItemType.MainWeapon) || item.isOwned)
                    getItem.Add(item);
            }
        }

        // 아이템 부족 시 카드 중복 출현
        List<ItemRuntimeData> picked = GetWeightRandomItem(getItem, count);

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

    public void SelectItem(ItemRuntimeData item)
    {
        item.isOwned = true;
        item.curLevel++;

        if (item.curLevel >= item.baseData.maxLevel)
            item.canAppear = false;

        item.Apply();

        Hide();
    }

    public ItemRuntimeData FindRuntimeWeapon(MainWeaponType weaponType)
    {
        foreach (var item in runtimeItems)
        {
            if (item.baseData.mainWeapon == weaponType)
                return item;
        }

        return null;
    }

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
