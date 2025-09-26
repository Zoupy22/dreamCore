using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(CharacterController))]
public class FirstPersonController : MonoBehaviour
{
    [Header("移动设置")]
    [SerializeField] private float moveSpeed = 5f;         // 移动速度
    [SerializeField] private float sprintSpeed = 8f;       //  sprint速度
    [SerializeField] private float rotationSpeed = 2f;     // 视角旋转速度
    [SerializeField] private float jumpHeight = 2f;        // 跳跃高度
    [SerializeField] private float gravity = -9.81f;       // 重力加速度

    [Header("地面检测")]
    [SerializeField] private Transform groundCheck;        // 地面检测点
    [SerializeField] private float groundDistance = 0.4f;  // 地面检测距离
    [SerializeField] private LayerMask groundMask;         // 地面层

    private CharacterController controller;
    private Camera playerCamera;
    private Vector3 velocity;
    private bool isGrounded;
    private float xRotation = 0f;

    private bool isFrozen = false;   // <-- 新增
    public void SetFrozen(bool frozen) => isFrozen = frozen;

    void Start()
    {
        // 获取组件引用
        controller = GetComponent<CharacterController>();
        playerCamera = GetComponentInChildren<Camera>();

        // 锁定鼠标到屏幕中心并隐藏
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        // ****** 冻结期间直接退出 ******
        if (isFrozen) return;
        // ********************************

        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
      

        // 当在地面上且速度向下时，重置垂直速度（防止累积重力）
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        // 鼠标视角控制
        MouseLook();

        // 移动控制
        Move();

        // 跳跃控制
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        // 应用重力
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    // 鼠标视角控制
    private void MouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * rotationSpeed;
        float mouseY = Input.GetAxis("Mouse Y") * rotationSpeed;

        // 垂直旋转（上下看）
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f); // 限制上下视角范围

        // 应用垂直旋转到相机
        playerCamera.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        // 水平旋转（左右看）- 旋转整个玩家
        transform.Rotate(Vector3.up * mouseX);
    }

    // 移动控制
    private void Move()
    {
        float x = Input.GetAxis("Horizontal"); // A/D 或 左右箭头
        float z = Input.GetAxis("Vertical");   // W/S 或 上下箭头

        // 根据当前视角计算移动方向
        Vector3 move = transform.right * x + transform.forward * z;

        // 归一化移动向量，防止斜向移动速度过快
        if (move.magnitude > 1)
        {
            move.Normalize();
        }

        // 检查是否 sprint
        float currentSpeed = Input.GetKey(KeyCode.LeftShift) ? sprintSpeed : moveSpeed;

        // 应用移动
        controller.Move(move * currentSpeed * Time.deltaTime);
    }

    // 绘制地面检测球的Gizmos
    private void OnDrawGizmos()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundDistance);
        }
    }
}
