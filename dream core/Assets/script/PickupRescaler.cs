using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

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
    private CommandBuffer cmdBuf;  // 额外渲染指令

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
        if (heldObject != null) return;          // 防止重复拾取

        Ray centerRay = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        if (!Physics.Raycast(centerRay, out RaycastHit hit, maxHoldDistance, grabbableLayers)) return;

        heldObject = hit.transform;
        heldObjectCollider = heldObject.GetComponent<Collider>();
        if (heldObjectCollider == null)
            heldObjectCollider = heldObject.gameObject.AddComponent<BoxCollider>();

        originalScale = heldObject.localScale;
        originalBounds = heldObjectCollider.bounds;
        originalDistance = Vector3.Distance(playerCamera.transform.position, heldObject.position);
        currentHoldDistance = originalDistance;

        heldObject.SetParent(playerCamera.transform);

        if (heldObject.TryGetComponent(out Rigidbody rb))
        {
            rb.isKinematic = true;
            rb.detectCollisions = true;
        }

        // 关闭阴影
        foreach (Renderer r in heldObject.GetComponentsInChildren<Renderer>())
        {
            r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            r.receiveShadows = false;
        }

        // 创建 CommandBuffer（如果已存在先清理）
        if (cmdBuf != null)
        {
            playerCamera.RemoveCommandBuffer(CameraEvent.AfterForwardOpaque, cmdBuf);
            cmdBuf.Dispose();
        }
        cmdBuf = new CommandBuffer();
        foreach (Renderer r in heldObject.GetComponentsInChildren<Renderer>())
            cmdBuf.DrawRenderer(r, r.material);
        playerCamera.AddCommandBuffer(CameraEvent.AfterForwardOpaque, cmdBuf);
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

        // 1. 清理 CommandBuffer
        if (cmdBuf != null)
        {
            playerCamera.RemoveCommandBuffer(CameraEvent.AfterForwardOpaque, cmdBuf);
            cmdBuf.Dispose();
            cmdBuf = null;
        }

        // 2. 用 SphereCast 检测障碍物
        Ray centerRay = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        float radius = heldObjectCollider ? heldObjectCollider.bounds.extents.magnitude : 0.5f;

        float maxDist = maxHoldDistance * 2f;
        float finalDist = maxDist;

        // 用球体射线检测，球面刚好贴墙
        if (Physics.SphereCast(centerRay, radius, out RaycastHit hit, maxDist, obstacleLayers))
        {
            finalDist = hit.distance;
        }

        finalDist = Mathf.Clamp(finalDist - minObstacleDistance, minHoldDistance, maxHoldDistance);
        heldObject.position = centerRay.GetPoint(finalDist);

        // 3. 其余逻辑不变
        heldObject.SetParent(null);
        if (heldObject.TryGetComponent(out Rigidbody rb))
        {
            rb.isKinematic = false;
            rb.velocity = centerRay.direction * 0.1f;
        }

        foreach (Renderer r in heldObject.GetComponentsInChildren<Renderer>())
        {
            r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
            r.receiveShadows = true;
        }

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

            if (heldObjectCollider != null)
            {
                Gizmos.color = Color.yellow;
                float radius = heldObjectCollider.bounds.extents.magnitude;
                Gizmos.DrawWireSphere(heldObject.position, radius);
            }
        }
    }
}