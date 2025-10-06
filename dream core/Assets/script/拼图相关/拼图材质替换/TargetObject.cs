using UnityEngine;

public class TargetObject : MonoBehaviour
{
    private GameObject currentHeldObject = null;

    private void OnTriggerEnter(Collider other)
    {
        // 检查进入的物体是否是手持物体（你可以用标签、层、或是否有 HeldObjectMaterial 来判断）
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
                    Debug.Log("材质已替换为：" + held.targetMaterial.name);
                }
            }
        }
    }
}