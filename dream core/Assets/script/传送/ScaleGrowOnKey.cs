using UnityEngine;

public class ScaleGrowFour_CustomKey : MonoBehaviour
{
    [Header("4 ������")]
    public Transform[] objs;   // �Ͻ���

    [Header("ÿ��������")]
    public float addScale = 0.2f;

    [Header("����ʱ��")]
    public float duration = 0.25f;

    [Header("�Զ��尴�������� Inspector ���ѡ����")]
    public KeyCode key = KeyCode.K;   // �� ���ʲô���͸�����

    // �ڲ���ֵ״̬
    float targetAdd = 0f;
    float currentAdd = 0f;
    float velocity = 0f;

    void Update()
    {
        // 1. ����Զ��尴��
        if (Input.GetKeyDown(key))
        {
            targetAdd += addScale;
        }

        // 2. ƽ����ֵ
        currentAdd = Mathf.SmoothDamp(currentAdd, targetAdd,
                                      ref velocity, duration);

        // 3. һ���Ը� 4 ������� scale.x
        foreach (var t in objs)
        {
            Vector3 s = t.localScale;
            s.x = 1f + currentAdd;   // ����ʼ���� 1�����ɳ�ʼֵ
            t.localScale = s;
        }
    }
}