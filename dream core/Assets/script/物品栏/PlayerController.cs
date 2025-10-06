using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance;

    [Header("持有位置")]
    public Transform holdPoint;          // 必须为玩家子物体

    [Header("前后调节参数")]
    public float scrollSpeed = 0.2f;
    public float minOffset = -1f;
    public float maxOffset = 1f;

    private GameObject currentHeldItem;
    private int currentSlotIndex = -1;

    /* 新增 */
    private float holdOffset = 0f;       // 相对初始位置的偏移量
    private Vector3 initialLocalPos;     // 场景里摆好的初始位置

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        // 记录设计师摆好的位置
        initialLocalPos = holdPoint.localPosition;
    }

    private void Update()
    {
        /* 快捷切换 */
        if (Input.GetKeyDown(KeyCode.Alpha1)) SwitchItem(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) SwitchItem(1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) SwitchItem(2);
        if (Input.GetKeyDown(KeyCode.Alpha4)) SwitchItem(3);

        /* 滚轮调节 */
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0)
        {
            holdOffset += scroll * scrollSpeed;
            holdOffset = Mathf.Clamp(holdOffset, minOffset, maxOffset);
        }
    }

    private void LateUpdate()
    {
        // 只在初始位置基础上做 Z 轴偏移
        holdPoint.localPosition = initialLocalPos + new Vector3(0, 0, holdOffset);
    }

    /* 切换物品：不再动 holdOffset */
    public void SwitchItem(int slotIndex)
    {
        /* 新增：如果按的是当前手持物品的槽位 → 放回并返回 */
        if (currentSlotIndex == slotIndex && currentHeldItem != null)
        {
            // 放回物品
            Inventory.Instance.SetItem(slotIndex, GetItemDataFromInstance(currentHeldItem));
            Destroy(currentHeldItem);
            currentHeldItem = null;
            currentSlotIndex = -1;
            return; // 直接返回，不再往下执行
        }

        /* 以下是原来的逻辑，未改动 */
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