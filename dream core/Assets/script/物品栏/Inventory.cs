using UnityEngine;
using UnityEngine.UI;

public class Inventory : MonoBehaviour
{
    public static Inventory Instance;
    public Image[] itemSlots; // ����4����Ʒ��UI
    private Item[] items = new Item[4]; // �洢��Ʒ����

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // �����Ʒ���ղ�λ
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
        Debug.Log("��Ʒ��������");
    }

    // ���²�λUI
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

    // ��ȡ��λ��Ʒ
    public Item GetItem(int index)
    {
        return (index >= 0 && index < 4) ? items[index] : null;
    }

    // ������Ʒ��ָ����λ
    public void SetItem(int index, Item item)
    {
        if (index >= 0 && index < 4)
        {
            items[index] = item;
            UpdateSlotUI(index);
        }
    }
}