using System.IO;
using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Camera))]   // 也可以手动拖 Camera
public class AutoShotToDesktop : MonoBehaviour
{
    [Header("拖入要渲染的 RenderTexture（可选）")]
    public RenderTexture rt;

    private Camera cam;

    void Start()
    {
        cam = GetComponent<Camera>();

        // 如果没有外部 RT，就临时现建一个
        if (rt == null)
        {
            rt = new RenderTexture(Screen.width, Screen.height, 24);
            cam.targetTexture = rt;
        }

        // 强制先渲染一帧，确保图像已生成
        StartCoroutine(ShotNextFrame());

        // 生成文件名
        string fileName = $"UnityShot_{System.DateTime.Now:yyyyMMdd_HHmmss}.png";

        // 桌面路径
        string desktopPath = Path.Combine(
            System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop),
            fileName);

        // 保存
        SaveRtToFile(rt, desktopPath);

        Debug.Log($"已保存到桌面：{desktopPath}");
    }

    /* 把 RenderTexture 存成 PNG */
    void SaveRtToFile(RenderTexture renderTex, string savePath)
    {
        Texture2D tex = new Texture2D(renderTex.width, renderTex.height, TextureFormat.RGB24, false);

        RenderTexture prev = RenderTexture.active;
        RenderTexture.active = renderTex;
        tex.ReadPixels(new Rect(0, 0, renderTex.width, renderTex.height), 0, 0);
        tex.Apply();
        RenderTexture.active = prev;

        byte[] bytes = tex.EncodeToPNG();
        File.WriteAllBytes(savePath, bytes);
        Destroy(tex);
    }
    IEnumerator ShotNextFrame()
    {
        // 等一帧，让所有渲染、UI、后处理都走完
        yield return new WaitForEndOfFrame();

        // 再渲染/抓图
        cam.Render();
        string fileName = $"UnityShot_{System.DateTime.Now:yyyyMMdd_HHmmss}.png";
        string desktopPath = Path.Combine(
            System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop),
            fileName);
        SaveRtToFile(rt, desktopPath);
        Debug.Log($"已保存到桌面：{desktopPath}");
    }
}