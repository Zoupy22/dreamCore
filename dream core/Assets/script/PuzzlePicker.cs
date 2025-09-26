using UnityEngine;

public class PuzzlePicker : MonoBehaviour
{
    [Header("吸附参数")]
    [SerializeField] private float pickRange = 4f;          // 射线最远距离
    [SerializeField] private float fixedDepth = 3f;         // 摄像机前方固定深度（米）

    [Header("放置容错")]
    [SerializeField] private float snapDistance = 0.8f;
    [SerializeField] private float snapAngle = 25f;

    private Camera cam;
    private Transform carried;      // 被抓的拼图
    private Rigidbody carriedRb;

    void Awake() => cam = GetComponent<Camera>();

    void Update()
    {
        // 只有拼图模式才工作
        if (!MouseModeManager.IsPuzzleMode) return;

        // 左键：吸 / 放
        if (Input.GetMouseButtonDown(0))
        {
            if (carried == null) TryPick();
            else TryPlace();
        }

        // 手上有拼图 → 直接瞬移到光标所在 3D 点
        if (carried != null)
            SnapToCursor();
    }

    // 把光标屏幕坐标转成 3D 世界点（固定深度）
    private Vector3 CursorWorldPos()
    {
        Vector3 screenPos = Input.mousePosition;
        screenPos.z = fixedDepth;                 // 关键：Z 就是深度
        return cam.ScreenToWorldPoint(screenPos);
    }

    // 每帧让拼图瞬移过去
    private void SnapToCursor()
    {
        carried.position = CursorWorldPos();
        // 角度保持吸起来那一刻，不动
    }

    #region 吸
    private void TryPick()
    {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, pickRange))
        {
            Transform t = hit.transform;
            if (!t.CompareTag("拼图1") && !t.CompareTag("拼图2") && !t.CompareTag("拼图3"))
                return;

            carried = t;
            carriedRb = t.GetComponent<Rigidbody>();
            carriedRb.isKinematic = true;
            carried.parent = null;
        }
    }
    #endregion

    #region 放
    private void TryPlace()
    {
        foreach (var slot in Physics.OverlapSphere(carried.position, snapDistance))
        {
            if (slot.CompareTag(carried.tag + "Slot") &&
                Quaternion.Angle(carried.rotation, slot.transform.rotation) < snapAngle)
            {
                DoSnap(carried, slot.transform);
                return;
            }
        }
        DropFree();
    }

    private void DoSnap(Transform piece, Transform slot)
    {
        piece.position = slot.position;
        piece.rotation = slot.rotation;

        var slotRend = slot.GetComponent<Renderer>();
        var pieceRend = piece.GetComponent<Renderer>();
        if (slotRend && pieceRend)
            slotRend.material = pieceRend.material;

        piece.gameObject.SetActive(false); // 拼完隐藏
        carried = null;
        carriedRb = null;
    }

    private void DropFree()
    {
        carriedRb.isKinematic = false;
        carried = null;
        carriedRb = null;
    }
    #endregion
}