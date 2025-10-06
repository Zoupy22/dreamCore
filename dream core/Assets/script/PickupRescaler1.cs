using UnityEngine;

[RequireComponent(typeof(Camera))]
public class PickupRescaler1 : MonoBehaviour
{
    [Header("��������")]
    public KeyCode toggleKey = KeyCode.E;

    [Header("��������")]
    public float minHoldDistance = 0.5f;
    public float maxHoldDistance = 20f;
    public float scrollSensitivity = 1f;
    [Tooltip("���ϰ������С��϶")]
    public float minObstacleDistance = 0.05f;
    public LayerMask grabbableLayers;
    public LayerMask obstacleLayers;

    // ========== ˽��״̬ ==========
    private Camera playerCamera;
    private Transform heldObject;
    private Vector3 originalScale;
    private float originalDistance;
    private float currentHoldDistance;
    private Collider heldCollider;
    private Bounds originalBounds;

    // ------------------------------------------------------------------
    void Awake() => playerCamera = GetComponent<Camera>();

    void Update()
    {
        // ʰȡ / ����
        if (Input.GetKeyDown(toggleKey))
        {
            if (heldObject == null) TryPickup();
            else Drop();
        }

        // �����ճ�
        if (heldObject != null) UpdateHeld();
    }

    // ==========================================================
    //  ʰȡ
    // ==========================================================
    void TryPickup()
    {
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        if (!Physics.Raycast(ray, out RaycastHit hit, maxHoldDistance, grabbableLayers))
            return;

        heldObject = hit.transform;
        heldCollider = heldObject.GetComponent<Collider>();
        if (heldCollider == null)
            heldCollider = heldObject.gameObject.AddComponent<BoxCollider>();

        originalScale = heldObject.localScale;
        originalBounds = heldCollider.bounds;
        originalDistance = hit.distance;
        currentHoldDistance = originalDistance;

        heldObject.SetParent(playerCamera.transform);

        if (heldObject.TryGetComponent(out Rigidbody rb))
        {
            rb.isKinematic = true;
            rb.detectCollisions = true;
        }

        // ����Ӱ
        foreach (var ren in heldObject.GetComponentsInChildren<Renderer>())
        {
            ren.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            ren.receiveShadows = false;
        }
    }

    // ==========================================================
    //  ÿ֡���£����ֵ��� + ����ģ
    // ==========================================================
    void UpdateHeld()
    {
        // ����
        currentHoldDistance += -Input.GetAxis("Mouse ScrollWheel") * scrollSensitivity;
        currentHoldDistance = Mathf.Clamp(currentHoldDistance, minHoldDistance, maxHoldDistance);

        // Ŀ��λ�ã���������ؿռ䣩
        Vector3 camLocalPos = new Vector3(0, 0, currentHoldDistance);
        Vector3 worldPos = playerCamera.transform.TransformPoint(camLocalPos);

        // ��ȫ�ƶ�
        SafeMoveToTarget(worldPos);

        // ���ţ������޺��ģ�
        float ratio = (heldObject.position - playerCamera.transform.position).magnitude
                      / originalDistance;
        heldObject.localScale = originalScale * ratio;
    }

    // --------------------------------------------------------------
    //  ��ȫ�ƶ���SphereCast + CheckSphere + Push-Out
    // --------------------------------------------------------------
    void SafeMoveToTarget(Vector3 targetWorldPos)
    {
        float radius = GetBoundingSphereRadius();
        Vector3 start = heldObject.position;
        Vector3 dir = (targetWorldPos - start);
        float distance = dir.magnitude;
        if (distance < 0.001f) return;
        dir /= distance;

        // 1) Sweep ���
        if (Physics.SphereCast(start, radius, dir, out RaycastHit hit, distance, obstacleLayers))
        {
            targetWorldPos = hit.point - hit.normal * (minObstacleDistance + radius);
        }

        // 2) Push-Out ����
        for (int i = 0; i < 3; i++)
        {
            if (Physics.CheckSphere(targetWorldPos, radius, obstacleLayers))
            {
                Vector3 push = (playerCamera.transform.position - targetWorldPos).normalized;
                targetWorldPos += push * (radius * 0.5f + minObstacleDistance);
            }
            else break;
        }

        heldObject.position = targetWorldPos;
    }

    // --------------------------------------------------------------
    //  �ܰ�ס��ǰ������ײ�����С��뾶���Ѽ��ݾɰ棩
    // --------------------------------------------------------------
    float GetBoundingSphereRadius()
    {
        if (heldCollider == null) return 0.1f;
        float maxScale = Mathf.Max(heldObject.localScale.x,
                                   Mathf.Max(heldObject.localScale.y,
                                            heldObject.localScale.z));
        return originalBounds.extents.magnitude * maxScale;
    }

    // ==========================================================
    //  ����
    // ==========================================================
    void Drop()
    {
        if (heldObject == null) return;

        // 1. ���㰲ȫ���µ�
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        float radius = GetBoundingSphereRadius();
        float maxDist = maxHoldDistance * 2f;
        float finalDist = maxDist;

        if (Physics.SphereCast(ray, radius, out RaycastHit hit, maxDist, obstacleLayers))
            finalDist = hit.distance;

        finalDist = Mathf.Clamp(finalDist - minObstacleDistance, minHoldDistance, maxHoldDistance);
        Vector3 dropPos = ray.GetPoint(finalDist);

        // 2. ǽ���� + ����
        for (int i = 0; i < 5 && IsBehindObstacle(dropPos); i++)
        {
            dropPos -= ray.direction * 0.2f;
        }

        // 3. Ӧ��
        heldObject.SetParent(null);
        heldObject.position = dropPos;

        if (heldObject.TryGetComponent(out Rigidbody rb))
        {
            rb.isKinematic = false;
            rb.velocity = ray.direction * 0.1f;
        }

        foreach (var ren in heldObject.GetComponentsInChildren<Renderer>())
        {
            ren.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
            ren.receiveShadows = true;
        }

        heldObject = null;
        heldCollider = null;
    }

    // --------------------------------------------------------------
    bool IsBehindObstacle(Vector3 point)
    {
        Vector3 toObj = point - playerCamera.transform.position;
        return Physics.Raycast(playerCamera.transform.position, toObj.normalized,
                               toObj.magnitude, obstacleLayers,
                               QueryTriggerInteraction.Ignore);
    }

    // ==========================================================
    //  Scene ��ͼ����
    // ==========================================================
    void OnDrawGizmos()
    {
        if (playerCamera == null) return;
        Gizmos.color = Color.cyan;
        Ray r = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        Gizmos.DrawRay(r.origin, r.direction * maxHoldDistance);

        if (heldObject != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(heldObject.position, GetBoundingSphereRadius());
        }
    }
}