using UnityEngine;

public class PuzzlePicker : MonoBehaviour
{
    [Header("��������")]
    [SerializeField] private float pickRange = 4f;          // ������Զ����
    [SerializeField] private float fixedDepth = 3f;         // �����ǰ���̶���ȣ��ף�

    [Header("�����ݴ�")]
    [SerializeField] private float snapDistance = 0.8f;
    [SerializeField] private float snapAngle = 25f;

    private Camera cam;
    private Transform carried;      // ��ץ��ƴͼ
    private Rigidbody carriedRb;

    void Awake() => cam = GetComponent<Camera>();

    void Update()
    {
        // ֻ��ƴͼģʽ�Ź���
        if (!MouseModeManager.IsPuzzleMode) return;

        // ������� / ��
        if (Input.GetMouseButtonDown(0))
        {
            if (carried == null) TryPick();
            else TryPlace();
        }

        // ������ƴͼ �� ֱ��˲�Ƶ�������� 3D ��
        if (carried != null)
            SnapToCursor();
    }

    // �ѹ����Ļ����ת�� 3D ����㣨�̶���ȣ�
    private Vector3 CursorWorldPos()
    {
        Vector3 screenPos = Input.mousePosition;
        screenPos.z = fixedDepth;                 // �ؼ���Z �������
        return cam.ScreenToWorldPoint(screenPos);
    }

    // ÿ֡��ƴͼ˲�ƹ�ȥ
    private void SnapToCursor()
    {
        carried.position = CursorWorldPos();
        // �Ƕȱ�����������һ�̣�����
    }

    #region ��
    private void TryPick()
    {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, pickRange))
        {
            Transform t = hit.transform;
            if (!t.CompareTag("ƴͼ1") && !t.CompareTag("ƴͼ2") && !t.CompareTag("ƴͼ3"))
                return;

            carried = t;
            carriedRb = t.GetComponent<Rigidbody>();
            carriedRb.isKinematic = true;
            carried.parent = null;
        }
    }
    #endregion

    #region ��
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

        piece.gameObject.SetActive(false); // ƴ������
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