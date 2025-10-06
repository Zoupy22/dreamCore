using UnityEngine;

public class Pickup : MonoBehaviour
{
    public Item item; // 拖入ScriptableObject物品数据

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player") && Input.GetKeyDown(KeyCode.E))
        {
            Inventory.Instance.AddItem(item);
            Destroy(gameObject); // 拾取后销毁场景中的物品
        }
    }
}