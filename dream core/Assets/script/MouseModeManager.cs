using UnityEngine;

public class MouseModeManager : MonoBehaviour
{
    public static bool IsPuzzleMode { get; private set; } = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            IsPuzzleMode = !IsPuzzleMode;
            // ����ƴͼģʽ �� ϵͳ������
            if (IsPuzzleMode)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            // �ص���һ�˳� �� ���ز�����
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }
    }
}