using UnityEngine;

public class Board : MonoBehaviour
{
    private Renderer rend;

    void Awake() => rend = GetComponent<Renderer>();

    // ֻҪ��Ƭ�����ң��ҡ����ڱ����š����ͻ�����
    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("PuzzlePiece")) return;

        PuzzlePiece p = other.GetComponent<PuzzlePiece>();
        if (p == null) return;
        if (!p.isHeld) return;          // ���롰���������Ч

        rend.material = p.GetMaterial();

        // ��ѡ����Ƭ��ʧ/����/��Ч
        // Destroy(p.gameObject);
    }
}