using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickupRescaler : MonoBehaviour
{
    [Header("控制设置")]
    public KeyCode toggleKey = KeyCode.E;  // 拾取/放下按键

    [Header("交互参数")]
    public float minHoldDistance = 0.5f;   // 最小握持距离
    public float maxHoldDistance = 20f;    // 最大握持距离
    public float scrollSensitivity = 1f;   // 滚轮灵敏度
    public float minObstacleDistance = 0.1f; // 与障碍物的最小距离
    public LayerMask grabbableLayers;      // 可抓取物体层
    public LayerMask obstacleLayers;       // 障碍物层

    private Camera playerCamera;
    private Transform heldObject;          // 当前握持的物体
    private Vector3 originalScale;         // 物体原始缩放
    private float originalDistance;        // 拾取时的初始距离
    private float currentHoldDistance;     // 当前握持距离
    private Collider heldObjectCollider;   // 握持物体的碰撞器
    private Bounds originalBounds;         // 原始边界

    void Awake()
    {
        playerCamera = GetComponent<Camera>();
    }

    void Update()
    {
        // 拾取/放下物体
        if (Input.GetKeyDown(toggleKey))
        {
            if (heldObject == null)
            {
                TryPickupObject();
            }
            else
            {
                DropObject();
            }
        }

        // 如果持有物体，更新其位置和缩放
        if (heldObject != null)
        {
            UpdateHeldObject();
        }
    }

    // 尝试拾取物体
    private void TryPickupObject()
    {
        Ray centerRay = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));

        // 只检测可抓取层的物体
        if (Physics.Raycast(centerRay, out RaycastHit hit, maxHoldDistance, grabbableLayers))
        {
            heldObject = hit.transform;

            // 获取物体碰撞器
            heldObjectCollider = heldObject.GetComponent<Collider>();
            if (heldObjectCollider == null)
            {
                // 如果没有碰撞器，尝试添加一个
                heldObjectCollider = heldObject.gameObject.AddComponent<BoxCollider>();
            }

            // 记录初始状态
            originalScale = heldObject.localScale;
            originalBounds = heldObjectCollider.bounds;
            originalDistance = Vector3.Distance(playerCamera.transform.position, heldObject.position);
            currentHoldDistance = originalDistance;

            // 设置物体父级为相机，方便跟随
            heldObject.SetParent(playerCamera.transform);

            // 禁用物理
            if (heldObject.TryGetComponent(out Rigidbody rb))
            {
                rb.isKinematic = true;
                rb.detectCollisions = true; // 确保碰撞检测仍然启用
            }
            // ====== 新增：关闭阴影 ======
            if (heldObject.TryGetComponent(out Renderer rend))
                rend.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            foreach (Renderer r in heldObject.GetComponentsInChildren<Renderer>())
            {
                r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                r.receiveShadows = false;   // 关键：不接收任何阴影
            }
        }
      
    }

    // 更新握持物体的位置和缩放
    private void UpdateHeldObject()
    {
        // 滚轮调距
        currentHoldDistance += Input.GetAxis("Mouse ScrollWheel") * -scrollSensitivity;
        currentHoldDistance = Mathf.Clamp(currentHoldDistance, minHoldDistance, maxHoldDistance);

        Ray centerRay = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));

        // 计算考虑物体大小的偏移量
        float objectSizeOffset = CalculateObjectSizeOffset(centerRay.direction);

        // 射线打到真正最远的障碍物
        float maxRay = maxHoldDistance * 2f;
        float actualDistance = currentHoldDistance;

        if (Physics.Raycast(centerRay, out RaycastHit hit, maxRay, obstacleLayers))
        {
            // 考虑物体大小，调整实际距离
            actualDistance = Mathf.Clamp(
                hit.distance - objectSizeOffset - minObstacleDistance,
                minHoldDistance,
                maxHoldDistance
            );
        }

        // 额外检查：确保物体不会穿过障碍物
        CheckAndResolveCollision(centerRay, ref actualDistance, objectSizeOffset);

        // 更新位置
        heldObject.localPosition = new Vector3(0, 0, actualDistance);

        // 更新缩放 (超阈限空间的核心效果)
        heldObject.localScale = originalScale * (actualDistance / originalDistance);

    }

    // 计算物体在射线方向上的大小偏移
    private float CalculateObjectSizeOffset(Vector3 direction)
    {
        if (heldObjectCollider == null) return 0;

        // 获取物体在当前缩放状态下的边界
        Bounds scaledBounds = originalBounds;
        scaledBounds.Expand(heldObject.localScale - Vector3.one);

        // 计算在射线方向上的边界延伸
        return Vector3.Dot(scaledBounds.extents, direction.normalized);
    }

    // 检查并解决碰撞
    private void CheckAndResolveCollision(Ray ray, ref float targetDistance, float objectSizeOffset)
    {
        // 计算物体应该在的位置
        Vector3 targetPosition = ray.origin + ray.direction * targetDistance;

        // 创建一个从当前位置到目标位置的射线
        Vector3 currentPosition = heldObject.position;
        Vector3 direction = targetPosition - currentPosition;
        float distance = direction.magnitude;

        if (distance > 0.001f)
        {
            // 发射一个包含物体半径的球体射线检测
            if (Physics.SphereCast(
                currentPosition,
                objectSizeOffset,
                direction.normalized,
                out RaycastHit hit,
                distance,
                obstacleLayers))
            {
                // 如果检测到碰撞，调整距离
                targetDistance = Vector3.Distance(ray.origin, currentPosition + direction.normalized * (hit.distance - minObstacleDistance));
                targetDistance = Mathf.Clamp(targetDistance, minHoldDistance, maxHoldDistance);
            }
        }
    }

    // 放下物体
    private void DropObject()
    {
        if (heldObject == null) return;

        // 从相机发射射线检测障碍物
        Ray centerRay = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));

        // 计算物体大小偏移
        float objectSizeOffset = CalculateObjectSizeOffset(centerRay.direction);

        // 检测障碍物层
        if (Physics.Raycast(centerRay, out RaycastHit hit, maxHoldDistance * 2, obstacleLayers))
        {
            // 精确放置在射线与障碍物的交点，考虑物体大小
            Vector3 targetPosition = hit.point - centerRay.direction * (objectSizeOffset + minObstacleDistance);
            heldObject.position = targetPosition;

            // 让物体贴合表面
            if (heldObject.TryGetComponent(out Renderer renderer))
            {
                // 计算物体边界到中心点的距离
                float offset = Vector3.Dot(renderer.bounds.extents, hit.normal);
                // 沿法线方向偏移，确保物体完全在障碍物外部
                heldObject.position += hit.normal * (offset + minObstacleDistance);
            }
        }
        else
        {
            // 如果没有检测到障碍物，放置在当前位置
            heldObject.position = playerCamera.transform.TransformPoint(heldObject.localPosition);
        }

        // 解除父子关系并恢复物理
        heldObject.SetParent(null);
        if (heldObject.TryGetComponent(out Rigidbody rb))
        {
            rb.isKinematic = false;
            // 给一个轻微的推力使其更自然
            rb.velocity = centerRay.direction * 0.1f;
        }
        // 恢复实时阴影
        // ================= 恢复两行：阴影 & 队列 =================
        foreach (Renderer r in heldObject.GetComponentsInChildren<Renderer>())
        {
            r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
            r.receiveShadows = true;
        }

        // 重置引用
        heldObject = null;
        heldObjectCollider = null;
       
    }

    // 场景视图绘制调试射线
    private void OnDrawGizmos()
    {
        if (playerCamera == null) return;

        Gizmos.color = Color.cyan;
        Ray centerRay = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));

        // 绘制交互射线
        Gizmos.DrawRay(centerRay.origin, centerRay.direction * maxHoldDistance);

        // 如果持有物体，绘制当前距离和碰撞边界
        if (heldObject != null)
        {
            Gizmos.color = Color.green;
            Vector3 targetPos = playerCamera.transform.TransformPoint(0, 0, currentHoldDistance);
            Gizmos.DrawWireSphere(targetPos, 0.1f);

            // 绘制物体大小偏移的调试球
            if (heldObjectCollider != null)
            {
                Gizmos.color = Color.yellow;
                float offset = CalculateObjectSizeOffset(centerRay.direction);
                Gizmos.DrawWireSphere(targetPos, offset);
            }
        }
    }
}