using UnityEngine;

public class PuzzlePiece : MonoBehaviour
{
    [HideInInspector] public bool isHeld = false;   // ÓÉÊ°È¡Æ÷ÉèÖÃ

    public Material GetMaterial() => GetComponent<Renderer>().material;
}