using UnityEngine;

public class Board : MonoBehaviour
{
    private Renderer rend;

    void Awake() => rend = GetComponent<Renderer>();

    // 只要碎片碰到我，且“正在被拿着”，就换材质
    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("PuzzlePiece")) return;

        PuzzlePiece p = other.GetComponent<PuzzlePiece>();
        if (p == null) return;
        if (!p.isHeld) return;          // 必须“在手里”才有效

        rend.material = p.GetMaterial();

        // 可选：碎片消失/放下/音效
        // Destroy(p.gameObject);
    }
}