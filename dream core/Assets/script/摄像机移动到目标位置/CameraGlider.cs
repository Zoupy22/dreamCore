using UnityEngine;

public class CameraGlider : MonoBehaviour
{
    [Header("Ŀ���λ")]
    [SerializeField] private Transform targetPose;   // �ںýǶ�λ�õĿ�����

    [Header("���в���")]
    [SerializeField] private float glideDuration = 1.2f;

    // �ڲ�״̬
    private bool isGliding = false;   // ���ڻ���
    private bool isLocked = false;   // �Ƿ��ڡ������ȴ��ڶ���R��״̬
    private Vector3 startPos;
    private Quaternion startRot;
    private float timer;

    // ����
    private FirstPersonController fpc;  // ���Լ��������

    void Awake()
    {
        fpc = FindObjectOfType<FirstPersonController>(); // ����������
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            if (!isLocked && !isGliding)   // ��һ�� R����ʼ��
                StartGlide();
            else if (isLocked)             // �ڶ��� R��ֱ�ӽ���
                Unlock();
        }

        if (isGliding)
            Glide();
    }

    void StartGlide()
    {
        isGliding = true;
        isLocked = false;
        timer = 0;
        startPos = transform.position;
        startRot = transform.rotation;

        fpc.SetFrozen(true);   // ��ס���
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Glide()
    {
        timer += Time.deltaTime;
        float t = Mathf.Clamp01(timer / glideDuration);
        t = 1f - Mathf.Pow(1f - t, 3);          // ease out

        transform.position = Vector3.Lerp(startPos, targetPose.position, t);
        transform.rotation = Quaternion.Slerp(startRot, targetPose.rotation, t);

        if (t >= 1f)           // ���꣬���롰�ȵڶ��� R��״̬
        {
            isGliding = false;
            isLocked = true;  // ������������һ�� R
        }
    }

    void Unlock()
    {
        isLocked = false;
        fpc.SetFrozen(false);  // �ָ���ҿ���
        Cursor.lockState = CursorLockMode.Locked; // �������ع�꣬����ԭ��ϰ��
        Cursor.visible = false;
    }
}