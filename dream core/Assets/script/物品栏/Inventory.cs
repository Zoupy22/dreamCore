using UnityEngine;
using UnityEngine.UI;

public class Inventory : MonoBehaviour
{
    public static Inventory Instance;
    public Image[] itemSlots; // 拖入4个物品槽UI
    private Item[] items = new Item[4]; // 存储物品数据

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // 添加物品到空槽位
    public void AddItem(Item item)
    {
        for (int i = 0; i < items.Length; i++)
        {
            if (items[i] == null)
            {
                items[i] = item;
                UpdateSlotUI(i);
                return;
            }
        }
        Debug.Log("物品栏已满！");
    }

    // 更新槽位UI
    private void UpdateSlotUI(int index)
    {
        if (items[index] != null)
        {
            itemSlots[index].sprite = items[index].icon;
            itemSlots[index].color = Color.white;
        }
        else
        {
            itemSlots[index].sprite = null;
            itemSlots[index].color = Color.clear;
        }
    }

    // 获取槽位物品
    public Item GetItem(int index)
    {
        return (index >= 0 && index < 4) ? items[index] : null;
    }

    // 放置物品到指定槽位
    public void SetItem(int index, Item item)
    {
        if (index >= 0 && index < 4)
        {
            items[index] = item;
            UpdateSlotUI(index);
        }
    }
}