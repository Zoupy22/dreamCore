using UnityEngine;

public class DoorController : MonoBehaviour
{
    [Header("门设置")]
    public float openAngle = 90f; // 开门时Y轴旋转角度（默认90度）
    public float rotateSpeed = 5f; // 旋转速度（数值越大越快）

    [Header("关联物体（开门时显示，关门时隐藏）")]
    public GameObject[] linkedObjects; // 拖入需要控制显隐的物体（数量自定义）

    [Header("检测范围")]
    public float checkRange = 2f; // 玩家需要在门附近多少距离内才能开门

    private bool isOpen = false; // 门当前状态（false=关闭，true=打开）
    private Quaternion closedRotation; // 门的初始关闭角度
    private Quaternion openRotation; // 门的打开角度
    private Transform player; // 玩家引用

    private void Start()
    {
        // 记录门的初始关闭角度
        closedRotation = transform.rotation;
        // 计算门的打开角度（在初始角度基础上Y轴加openAngle）
        openRotation = Quaternion.Euler(
            closedRotation.eulerAngles.x,
            closedRotation.eulerAngles.y + openAngle,
            closedRotation.eulerAngles.z
        );

        // 找到玩家（确保玩家标签为"Player"）
        player = GameObject.FindGameObjectWithTag("Player").transform;

        // 初始状态：隐藏关联物体
        SetLinkedObjectsActive(false);
    }

    private void Update()
    {
        // 检测玩家是否在范围内，且按了E键
        if (IsPlayerInRange() && Input.GetKeyDown(KeyCode.E))
        {
            // 切换门状态（开→关，关→开）
            isOpen = !isOpen;
            // 同步控制关联物体显隐
            SetLinkedObjectsActive(isOpen);
        }

        // 平滑旋转门到目标角度
        if (isOpen)
        {
            // 旋转到打开角度
            transform.rotation = Quaternion.Lerp(transform.rotation, openRotation, rotateSpeed * Time.deltaTime);
        }
        else
        {
            // 旋转到关闭角度
            transform.rotation = Quaternion.Lerp(transform.rotation, closedRotation, rotateSpeed * Time.deltaTime);
        }
    }

    // 检查玩家是否在门的有效范围内
    private bool IsPlayerInRange()
    {
        if (player == null) return false;
        // 计算玩家与门的距离
        float distance = Vector3.Distance(transform.position, player.position);
        return distance <= checkRange;
    }

    // 控制关联物体的显隐
    private void SetLinkedObjectsActive(bool active)
    {
        foreach (GameObject obj in linkedObjects)
        {
            if (obj != null) // 防止空引用错误
            {
                obj.SetActive(active);
            }
        }
    }

    // 可选：在Scene窗口显示检测范围（方便调整）
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        // 绘制一个球体表示检测范围
        Gizmos.DrawWireSphere(transform.position, checkRange);
    }
}