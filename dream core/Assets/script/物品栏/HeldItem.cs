using UnityEngine;

public class HeldItem : MonoBehaviour
{
    public int slotIndex;

    private void Awake()
    {
        // ȷ���ֳ���Ʒ����ײ�壨���Ԥ����û�еĻ���
        Collider col = GetComponent<Collider>();
        if (col == null)
        {
            col = gameObject.AddComponent<SphereCollider>();
        }
        // ȷ����ײ�岻�Ǵ���������Ҫ������ײ��⣩
        col.isTrigger = false;

        // ��Ӹ��壨ȷ����ײ��Ч��
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
            rb.isKinematic = true; // ��������ƶ�����������Ӱ��
        }
    }
}