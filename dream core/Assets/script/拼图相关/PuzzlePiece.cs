using UnityEngine;

public class PuzzlePiece : MonoBehaviour
{
    [HideInInspector] public bool isHeld = false;   // ��ʰȡ������

    public Material GetMaterial() => GetComponent<Renderer>().material;
}