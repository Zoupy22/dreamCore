using UnityEngine;
using CharacterController = UnityEngine.CharacterController; // 处理角色控制器特殊情况

public class TeleportZone : MonoBehaviour
{
    public Transform targetPoint; // 传送目标点（必须设置）
    [Tooltip("如果玩家用了CharacterController，需要勾选")]
    public bool useCharacterController; // 角色控制器专用开关

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // 强制获取玩家根对象（避免子物体碰撞导致错误）
            Transform playerRoot = other.transform.root;
            Teleport(playerRoot);
        }
    }

    private void Teleport(Transform player)
    {
        if (targetPoint == null)
        {
            Debug.LogError("请先设置targetPoint！");
            return;
        }

        // 1. 处理角色控制器（CharacterController会阻止直接改position）
        if (useCharacterController)
        {
            CharacterController cc = player.GetComponent<CharacterController>();
            if (cc != null)
            {
                cc.enabled = false; // 临时禁用控制器，允许修改位置
                player.position = targetPoint.position; // 直接改位置
                cc.enabled = true; // 重新启用控制器
            }
        }
        // 2. 普通情况（刚体或无特殊组件）
        else
        {
            player.position = targetPoint.position; // 直接修改位置
        }

        // 同步旋转（可选）
        player.rotation = targetPoint.rotation;
        Debug.Log($"已将玩家传送到: {targetPoint.position}");
    }
}