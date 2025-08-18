using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        Ray centerRay = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));

        // ֻ����ץȡ�������
        if (Physics.Raycast(centerRay, out RaycastHit hit, maxHoldDistance, grabbableLayers))
        {
            heldObject = hit.transform;

            // ��ȡ������ײ��
            heldObjectCollider = heldObject.GetComponent<Collider>();
            if (heldObjectCollider == null)
            {
                // ���û����ײ�����������һ��
                heldObjectCollider = heldObject.gameObject.AddComponent<BoxCollider>();
            }

            // ��¼��ʼ״̬
            originalScale = heldObject.localScale;
            originalBounds = heldObjectCollider.bounds;
            originalDistance = Vector3.Distance(playerCamera.transform.position, heldObject.position);
            currentHoldDistance = originalDistance;

            // �������常��Ϊ������������
            heldObject.SetParent(playerCamera.transform);

            // ��������
            if (heldObject.TryGetComponent(out Rigidbody rb))
            {
                rb.isKinematic = true;
                rb.detectCollisions = true; // ȷ����ײ�����Ȼ����
            }
            // ====== �������ر���Ӱ ======
            if (heldObject.TryGetComponent(out Renderer rend))
                rend.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            foreach (Renderer r in heldObject.GetComponentsInChildren<Renderer>())
            {
                r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                r.receiveShadows = false;   // �ؼ����������κ���Ӱ
            }
        }
      
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

        // ������������߼���ϰ���
        Ray centerRay = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));

        // ���������Сƫ��
        float objectSizeOffset = CalculateObjectSizeOffset(centerRay.direction);

        // ����ϰ����
        if (Physics.Raycast(centerRay, out RaycastHit hit, maxHoldDistance * 2, obstacleLayers))
        {
            // ��ȷ�������������ϰ���Ľ��㣬���������С
            Vector3 targetPosition = hit.point - centerRay.direction * (objectSizeOffset + minObstacleDistance);
            heldObject.position = targetPosition;

            // ���������ϱ���
            if (heldObject.TryGetComponent(out Renderer renderer))
            {
                // ��������߽絽���ĵ�ľ���
                float offset = Vector3.Dot(renderer.bounds.extents, hit.normal);
                // �ط��߷���ƫ�ƣ�ȷ��������ȫ���ϰ����ⲿ
                heldObject.position += hit.normal * (offset + minObstacleDistance);
            }
        }
        else
        {
            // ���û�м�⵽�ϰ�������ڵ�ǰλ��
            heldObject.position = playerCamera.transform.TransformPoint(heldObject.localPosition);
        }

        // ������ӹ�ϵ���ָ�����
        heldObject.SetParent(null);
        if (heldObject.TryGetComponent(out Rigidbody rb))
        {
            rb.isKinematic = false;
            // ��һ����΢������ʹ�����Ȼ
            rb.velocity = centerRay.direction * 0.1f;
        }
        // �ָ�ʵʱ��Ӱ
        // ================= �ָ����У���Ӱ & ���� =================
        foreach (Renderer r in heldObject.GetComponentsInChildren<Renderer>())
        {
            r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
            r.receiveShadows = true;
        }

        // ��������
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

            // ���������Сƫ�Ƶĵ�����
            if (heldObjectCollider != null)
            {
                Gizmos.color = Color.yellow;
                float offset = CalculateObjectSizeOffset(centerRay.direction);
                Gizmos.DrawWireSphere(targetPos, offset);
            }
        }
    }
}