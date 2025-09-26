using UnityEngine;

public class CameraGlider : MonoBehaviour
{
    [Header("目标机位")]
    [SerializeField] private Transform targetPose;   // 摆好角度位置的空物体

    [Header("滑行参数")]
    [SerializeField] private float glideDuration = 1.2f;

    // 内部状态
    private bool isGliding = false;   // 正在滑？
    private bool isLocked = false;   // 是否处于“锁定等待第二次R”状态
    private Vector3 startPos;
    private Quaternion startRot;
    private float timer;

    // 引用
    private FirstPersonController fpc;  // 拖自己玩家物体

    void Awake()
    {
        fpc = FindObjectOfType<FirstPersonController>(); // 或者拖引用
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            if (!isLocked && !isGliding)   // 第一次 R：开始滑
                StartGlide();
            else if (isLocked)             // 第二次 R：直接解锁
                Unlock();
        }

        if (isGliding)
            Glide();
    }

    void StartGlide()
    {
        isGliding = true;
        isLocked = false;
        timer = 0;
        startPos = transform.position;
        startRot = transform.rotation;

        fpc.SetFrozen(true);   // 锁住玩家
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Glide()
    {
        timer += Time.deltaTime;
        float t = Mathf.Clamp01(timer / glideDuration);
        t = 1f - Mathf.Pow(1f - t, 3);          // ease out

        transform.position = Vector3.Lerp(startPos, targetPose.position, t);
        transform.rotation = Quaternion.Slerp(startRot, targetPose.rotation, t);

        if (t >= 1f)           // 滑完，进入“等第二次 R”状态
        {
            isGliding = false;
            isLocked = true;  // 不解锁，等下一次 R
        }
    }

    void Unlock()
    {
        isLocked = false;
        fpc.SetFrozen(false);  // 恢复玩家控制
        Cursor.lockState = CursorLockMode.Locked; // 保持隐藏光标，按你原来习惯
        Cursor.visible = false;
    }
}