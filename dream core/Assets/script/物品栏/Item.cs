using UnityEngine;

[CreateAssetMenu(fileName = "NewItem", menuName = "Inventory/Item")]
public class Item : ScriptableObject // ����ScriptableObject�洢���ݣ��������ö�ʧ
{
    public string itemName;
    public Sprite icon;
    public GameObject prefab; // ��Ʒʵ��Ԥ���壨���븳ֵ��
   
}