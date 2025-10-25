using UnityEngine;

public class DoorController : MonoBehaviour
{
    [Header("������")]
    public float openAngle = 90f; // ����ʱY����ת�Ƕȣ�Ĭ��90�ȣ�
    public float rotateSpeed = 5f; // ��ת�ٶȣ���ֵԽ��Խ�죩

    [Header("�������壨����ʱ��ʾ������ʱ���أ�")]
    public GameObject[] linkedObjects; // ������Ҫ�������������壨�����Զ��壩

    [Header("��ⷶΧ")]
    public float checkRange = 2f; // �����Ҫ���Ÿ������پ����ڲ��ܿ���

    private bool isOpen = false; // �ŵ�ǰ״̬��false=�رգ�true=�򿪣�
    private Quaternion closedRotation; // �ŵĳ�ʼ�رսǶ�
    private Quaternion openRotation; // �ŵĴ򿪽Ƕ�
    private Transform player; // �������

    private void Start()
    {
        // ��¼�ŵĳ�ʼ�رսǶ�
        closedRotation = transform.rotation;
        // �����ŵĴ򿪽Ƕȣ��ڳ�ʼ�ǶȻ�����Y���openAngle��
        openRotation = Quaternion.Euler(
            closedRotation.eulerAngles.x,
            closedRotation.eulerAngles.y + openAngle,
            closedRotation.eulerAngles.z
        );

        // �ҵ���ң�ȷ����ұ�ǩΪ"Player"��
        player = GameObject.FindGameObjectWithTag("Player").transform;

        // ��ʼ״̬�����ع�������
        SetLinkedObjectsActive(false);
    }

    private void Update()
    {
        // �������Ƿ��ڷ�Χ�ڣ��Ұ���E��
        if (IsPlayerInRange() && Input.GetKeyDown(KeyCode.E))
        {
            // �л���״̬�������أ��ء�����
            isOpen = !isOpen;
            // ͬ�����ƹ�����������
            SetLinkedObjectsActive(isOpen);
        }

        // ƽ����ת�ŵ�Ŀ��Ƕ�
        if (isOpen)
        {
            // ��ת���򿪽Ƕ�
            transform.rotation = Quaternion.Lerp(transform.rotation, openRotation, rotateSpeed * Time.deltaTime);
        }
        else
        {
            // ��ת���رսǶ�
            transform.rotation = Quaternion.Lerp(transform.rotation, closedRotation, rotateSpeed * Time.deltaTime);
        }
    }

    // �������Ƿ����ŵ���Ч��Χ��
    private bool IsPlayerInRange()
    {
        if (player == null) return false;
        // ����������ŵľ���
        float distance = Vector3.Distance(transform.position, player.position);
        return distance <= checkRange;
    }

    // ���ƹ������������
    private void SetLinkedObjectsActive(bool active)
    {
        foreach (GameObject obj in linkedObjects)
        {
            if (obj != null) // ��ֹ�����ô���
            {
                obj.SetActive(active);
            }
        }
    }

    // ��ѡ����Scene������ʾ��ⷶΧ�����������
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        // ����һ�������ʾ��ⷶΧ
        Gizmos.DrawWireSphere(transform.position, checkRange);
    }
}