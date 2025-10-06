using UnityEngine;

public class HeldItem : MonoBehaviour
{
    public int slotIndex;

    private void Awake()
    {
        // 确保手持物品有碰撞体（如果预制体没有的话）
        Collider col = GetComponent<Collider>();
        if (col == null)
        {
            col = gameObject.AddComponent<SphereCollider>();
        }
        // 确保碰撞体不是触发器（需要物理碰撞检测）
        col.isTrigger = false;

        // 添加刚体（确保碰撞有效）
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
            rb.isKinematic = true; // 跟随玩家移动，不受物理影响
        }
    }
}