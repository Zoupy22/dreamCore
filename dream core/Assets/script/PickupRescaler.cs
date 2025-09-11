using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class PickupRescaler : MonoBehaviour
{
    [Header("��������")]
    public KeyCode toggleKey = KeyCode.E;  // ʰȡ/���°���

    [Header("��������")]
    public float minHoldDistance = 0.5f;   // ��С�ճ־���
    public float maxHoldDistance = 20f;    // ����ճ־���
    public float scrollSensitivity = 1f;   // ����������
    public float minObstacleDistance = 0.1f; // ���ϰ������С����
    public LayerMask grabbableLayers;      // ��ץȡ�����
    public LayerMask obstacleLayers;       // �ϰ����

    private Camera playerCamera;
    private Transform heldObject;          // ��ǰ�ճֵ�����
    private Vector3 originalScale;         // ����ԭʼ����
    private float originalDistance;        // ʰȡʱ�ĳ�ʼ����
    private float currentHoldDistance;     // ��ǰ�ճ־���
    private Collider heldObjectCollider;   // �ճ��������ײ��
    private Bounds originalBounds;         // ԭʼ�߽�
    private CommandBuffer cmdBuf;  // ������Ⱦָ��

    void Awake()
    {
        playerCamera = GetComponent<Camera>();
    }

    void Update()
    {
        // ʰȡ/��������
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

        // ����������壬������λ�ú�����
        if (heldObject != null)
        {
            UpdateHeldObject();
        }
    }

    // ����ʰȡ����
    private void TryPickupObject()
    {
        if (heldObject != null) return;          // ��ֹ�ظ�ʰȡ

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

        // �ر���Ӱ
        foreach (Renderer r in heldObject.GetComponentsInChildren<Renderer>())
        {
            r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            r.receiveShadows = false;
        }

        // ���� CommandBuffer������Ѵ���������
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

    // �����ճ������λ�ú�����
    private void UpdateHeldObject()
    {
        // ���ֵ���
        currentHoldDistance += Input.GetAxis("Mouse ScrollWheel") * -scrollSensitivity;
        currentHoldDistance = Mathf.Clamp(currentHoldDistance, minHoldDistance, maxHoldDistance);

        Ray centerRay = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));

        // ���㿼�������С��ƫ����
        float objectSizeOffset = CalculateObjectSizeOffset(centerRay.direction);

        // ���ߴ�������Զ���ϰ���
        float maxRay = maxHoldDistance * 2f;
        float actualDistance = currentHoldDistance;

        if (Physics.Raycast(centerRay, out RaycastHit hit, maxRay, obstacleLayers))
        {
            // ���������С������ʵ�ʾ���
            actualDistance = Mathf.Clamp(
                hit.distance - objectSizeOffset - minObstacleDistance,
                minHoldDistance,
                maxHoldDistance
            );
        }

        // �����飺ȷ�����岻�ᴩ���ϰ���
        CheckAndResolveCollision(centerRay, ref actualDistance, objectSizeOffset);

        // ����λ��
        heldObject.localPosition = new Vector3(0, 0, actualDistance);

        // �������� (�����޿ռ�ĺ���Ч��)
        heldObject.localScale = originalScale * (actualDistance / originalDistance);

    }

    // �������������߷����ϵĴ�Сƫ��
    private float CalculateObjectSizeOffset(Vector3 direction)
    {
        if (heldObjectCollider == null) return 0;

        // ��ȡ�����ڵ�ǰ����״̬�µı߽�
        Bounds scaledBounds = originalBounds;
        scaledBounds.Expand(heldObject.localScale - Vector3.one);

        // ���������߷����ϵı߽�����
        return Vector3.Dot(scaledBounds.extents, direction.normalized);
    }

    // ��鲢�����ײ
    private void CheckAndResolveCollision(Ray ray, ref float targetDistance, float objectSizeOffset)
    {
        // ��������Ӧ���ڵ�λ��
        Vector3 targetPosition = ray.origin + ray.direction * targetDistance;

        // ����һ���ӵ�ǰλ�õ�Ŀ��λ�õ�����
        Vector3 currentPosition = heldObject.position;
        Vector3 direction = targetPosition - currentPosition;
        float distance = direction.magnitude;

        if (distance > 0.001f)
        {
            // ����һ����������뾶���������߼��
            if (Physics.SphereCast(
                currentPosition,
                objectSizeOffset,
                direction.normalized,
                out RaycastHit hit,
                distance,
                obstacleLayers))
            {
                // �����⵽��ײ����������
                targetDistance = Vector3.Distance(ray.origin, currentPosition + direction.normalized * (hit.distance - minObstacleDistance));
                targetDistance = Mathf.Clamp(targetDistance, minHoldDistance, maxHoldDistance);
            }
        }
    }

    // ��������
    private void DropObject()
    {
        if (heldObject == null) return;

        // 1. ���� CommandBuffer
        if (cmdBuf != null)
        {
            playerCamera.RemoveCommandBuffer(CameraEvent.AfterForwardOpaque, cmdBuf);
            cmdBuf.Dispose();
            cmdBuf = null;
        }

        // 2. �� SphereCast ����ϰ���
        Ray centerRay = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        float radius = heldObjectCollider ? heldObjectCollider.bounds.extents.magnitude : 0.5f;

        float maxDist = maxHoldDistance * 2f;
        float finalDist = maxDist;

        // ���������߼�⣬����պ���ǽ
        if (Physics.SphereCast(centerRay, radius, out RaycastHit hit, maxDist, obstacleLayers))
        {
            finalDist = hit.distance;
        }

        finalDist = Mathf.Clamp(finalDist - minObstacleDistance, minHoldDistance, maxHoldDistance);
        heldObject.position = centerRay.GetPoint(finalDist);

        // 3. �����߼�����
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

    // ������ͼ���Ƶ�������
    private void OnDrawGizmos()
    {
        if (playerCamera == null) return;

        Gizmos.color = Color.cyan;
        Ray centerRay = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));

        // ���ƽ�������
        Gizmos.DrawRay(centerRay.origin, centerRay.direction * maxHoldDistance);

        // ����������壬���Ƶ�ǰ�������ײ�߽�
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