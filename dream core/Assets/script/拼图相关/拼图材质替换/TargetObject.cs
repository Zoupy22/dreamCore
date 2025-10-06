using UnityEngine;

public class TargetObject : MonoBehaviour
{
    private GameObject currentHeldObject = null;

    private void OnTriggerEnter(Collider other)
    {
        // ������������Ƿ����ֳ����壨������ñ�ǩ���㡢���Ƿ��� HeldObjectMaterial ���жϣ�
        HeldObjectMaterial held = other.GetComponent<HeldObjectMaterial>();
        if (held != null)
        {
            currentHeldObject = other.gameObject;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject == currentHeldObject)
        {
            currentHeldObject = null;
        }
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0) && currentHeldObject != null)
        {
            HeldObjectMaterial held = currentHeldObject.GetComponent<HeldObjectMaterial>();
            if (held != null && held.targetMaterial != null)
            {
                Renderer renderer = GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.material = held.targetMaterial;
                    Debug.Log("�������滻Ϊ��" + held.targetMaterial.name);
                }
            }
        }
    }
}