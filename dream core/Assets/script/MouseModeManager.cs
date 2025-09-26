using UnityEngine;

public class MouseModeManager : MonoBehaviour
{
    public static bool IsPuzzleMode { get; private set; } = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            IsPuzzleMode = !IsPuzzleMode;
            // 进入拼图模式 → 系统光标出现
            if (IsPuzzleMode)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            // 回到第一人称 → 隐藏并锁定
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }
    }
}