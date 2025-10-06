using UnityEngine;

[CreateAssetMenu(fileName = "NewItem", menuName = "Inventory/Item")]
public class Item : ScriptableObject // 改用ScriptableObject存储数据，避免引用丢失
{
    public string itemName;
    public Sprite icon;
    public GameObject prefab; // 物品实体预制体（必须赋值）
   
}