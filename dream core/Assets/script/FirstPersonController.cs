using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(CharacterController))]
public class FirstPersonController : MonoBehaviour
{
    [Header("�ƶ�����")]
    [SerializeField] private float moveSpeed = 5f;         // �ƶ��ٶ�
    [SerializeField] private float sprintSpeed = 8f;       //  sprint�ٶ�
    [SerializeField] private float rotationSpeed = 2f;     // �ӽ���ת�ٶ�
    [SerializeField] private float jumpHeight = 2f;        // ��Ծ�߶�
    [SerializeField] private float gravity = -9.81f;       // �������ٶ�

    [Header("������")]
    [SerializeField] private Transform groundCheck;        // �������
    [SerializeField] private float groundDistance = 0.4f;  // ���������
    [SerializeField] private LayerMask groundMask;         // �����

    private CharacterController controller;
    private Camera playerCamera;
    private Vector3 velocity;
    private bool isGrounded;
    private float xRotation = 0f;

    private bool isFrozen = false;   // <-- ����
    public void SetFrozen(bool frozen) => isFrozen = frozen;

    void Start()
    {
        // ��ȡ�������
        controller = GetComponent<CharacterController>();
        playerCamera = GetComponentInChildren<Camera>();

        // ������굽��Ļ���Ĳ�����
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        // ****** �����ڼ�ֱ���˳� ******
        if (isFrozen) return;
        // ********************************

        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
      

        // ���ڵ��������ٶ�����ʱ�����ô�ֱ�ٶȣ���ֹ�ۻ�������
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        // ����ӽǿ���
        MouseLook();

        // �ƶ�����
        Move();

        // ��Ծ����
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        // Ӧ������
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    // ����ӽǿ���
    private void MouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * rotationSpeed;
        float mouseY = Input.GetAxis("Mouse Y") * rotationSpeed;

        // ��ֱ��ת�����¿���
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f); // ���������ӽǷ�Χ

        // Ӧ�ô�ֱ��ת�����
        playerCamera.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        // ˮƽ��ת�����ҿ���- ��ת�������
        transform.Rotate(Vector3.up * mouseX);
    }

    // �ƶ�����
    private void Move()
    {
        float x = Input.GetAxis("Horizontal"); // A/D �� ���Ҽ�ͷ
        float z = Input.GetAxis("Vertical");   // W/S �� ���¼�ͷ

        // ���ݵ�ǰ�ӽǼ����ƶ�����
        Vector3 move = transform.right * x + transform.forward * z;

        // ��һ���ƶ���������ֹб���ƶ��ٶȹ���
        if (move.magnitude > 1)
        {
            move.Normalize();
        }

        // ����Ƿ� sprint
        float currentSpeed = Input.GetKey(KeyCode.LeftShift) ? sprintSpeed : moveSpeed;

        // Ӧ���ƶ�
        controller.Move(move * currentSpeed * Time.deltaTime);
    }

    // ���Ƶ��������Gizmos
    private void OnDrawGizmos()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundDistance);
        }
    }
}
