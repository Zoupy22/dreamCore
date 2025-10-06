using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance;

    [Header("����λ��")]
    public Transform holdPoint;          // ����Ϊ���������

    [Header("ǰ����ڲ���")]
    public float scrollSpeed = 0.2f;
    public float minOffset = -1f;
    public float maxOffset = 1f;

    private GameObject currentHeldItem;
    private int currentSlotIndex = -1;

    /* ���� */
    private float holdOffset = 0f;       // ��Գ�ʼλ�õ�ƫ����
    private Vector3 initialLocalPos;     // ������ںõĳ�ʼλ��

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        // ��¼���ʦ�ںõ�λ��
        initialLocalPos = holdPoint.localPosition;
    }

    private void Update()
    {
        /* ����л� */
        if (Input.GetKeyDown(KeyCode.Alpha1)) SwitchItem(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) SwitchItem(1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) SwitchItem(2);
        if (Input.GetKeyDown(KeyCode.Alpha4)) SwitchItem(3);

        /* ���ֵ��� */
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0)
        {
            holdOffset += scroll * scrollSpeed;
            holdOffset = Mathf.Clamp(holdOffset, minOffset, maxOffset);
        }
    }

    private void LateUpdate()
    {
        // ֻ�ڳ�ʼλ�û������� Z ��ƫ��
        holdPoint.localPosition = initialLocalPos + new Vector3(0, 0, holdOffset);
    }

    /* �л���Ʒ�����ٶ� holdOffset */
    public void SwitchItem(int slotIndex)
    {
        /* ��������������ǵ�ǰ�ֳ���Ʒ�Ĳ�λ �� �Żز����� */
        if (currentSlotIndex == slotIndex && currentHeldItem != null)
        {
            // �Ż���Ʒ
            Inventory.Instance.SetItem(slotIndex, GetItemDataFromInstance(currentHeldItem));
            Destroy(currentHeldItem);
            currentHeldItem = null;
            currentSlotIndex = -1;
            return; // ֱ�ӷ��أ���������ִ��
        }

        /* ������ԭ�����߼���δ�Ķ� */
        if (currentHeldItem != null && currentSlotIndex != -1)
        {
            Inventory.Instance.SetItem(currentSlotIndex, GetItemDataFromInstance(currentHeldItem));
            Destroy(currentHeldItem);
            currentHeldItem = null;
        }

        Item newItem = Inventory.Instance.GetItem(slotIndex);
        if (newItem != null)
        {
            currentHeldItem = Instantiate(newItem.prefab, holdPoint.position, holdPoint.rotation);
            currentHeldItem.transform.SetParent(holdPoint);
            currentHeldItem.AddComponent<ItemInstance>().itemData = newItem;
            Inventory.Instance.SetItem(slotIndex, null);
            currentSlotIndex = slotIndex;
        }
        else
        {
            currentSlotIndex = -1;
        }
    }

    private Item GetItemDataFromInstance(GameObject instance)
    {
        var inst = instance.GetComponent<ItemInstance>();
        return inst != null ? inst.itemData : null;
    }
}