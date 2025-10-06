using UnityEngine;

public class Pickup : MonoBehaviour
{
    public Item item; // ����ScriptableObject��Ʒ����

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player") && Input.GetKeyDown(KeyCode.E))
        {
            Inventory.Instance.AddItem(item);
            Destroy(gameObject); // ʰȡ�����ٳ����е���Ʒ
        }
    }
}