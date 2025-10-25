using UnityEngine;
using CharacterController = UnityEngine.CharacterController; // �����ɫ�������������

public class TeleportZone : MonoBehaviour
{
    public Transform targetPoint; // ����Ŀ��㣨�������ã�
    [Tooltip("����������CharacterController����Ҫ��ѡ")]
    public bool useCharacterController; // ��ɫ������ר�ÿ���

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // ǿ�ƻ�ȡ��Ҹ����󣨱�����������ײ���´���
            Transform playerRoot = other.transform.root;
            Teleport(playerRoot);
        }
    }

    private void Teleport(Transform player)
    {
        if (targetPoint == null)
        {
            Debug.LogError("��������targetPoint��");
            return;
        }

        // 1. �����ɫ��������CharacterController����ֱֹ�Ӹ�position��
        if (useCharacterController)
        {
            CharacterController cc = player.GetComponent<CharacterController>();
            if (cc != null)
            {
                cc.enabled = false; // ��ʱ���ÿ������������޸�λ��
                player.position = targetPoint.position; // ֱ�Ӹ�λ��
                cc.enabled = true; // �������ÿ�����
            }
        }
        // 2. ��ͨ���������������������
        else
        {
            player.position = targetPoint.position; // ֱ���޸�λ��
        }

        // ͬ����ת����ѡ��
        player.rotation = targetPoint.rotation;
        Debug.Log($"�ѽ���Ҵ��͵�: {targetPoint.position}");
    }
}