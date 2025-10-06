using System.IO;
using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Camera))]   // Ҳ�����ֶ��� Camera
public class AutoShotToDesktop : MonoBehaviour
{
    [Header("����Ҫ��Ⱦ�� RenderTexture����ѡ��")]
    public RenderTexture rt;

    private Camera cam;

    void Start()
    {
        cam = GetComponent<Camera>();

        // ���û���ⲿ RT������ʱ�ֽ�һ��
        if (rt == null)
        {
            rt = new RenderTexture(Screen.width, Screen.height, 24);
            cam.targetTexture = rt;
        }

        // ǿ������Ⱦһ֡��ȷ��ͼ��������
        StartCoroutine(ShotNextFrame());

        // �����ļ���
        string fileName = $"UnityShot_{System.DateTime.Now:yyyyMMdd_HHmmss}.png";

        // ����·��
        string desktopPath = Path.Combine(
            System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop),
            fileName);

        // ����
        SaveRtToFile(rt, desktopPath);

        Debug.Log($"�ѱ��浽���棺{desktopPath}");
    }

    /* �� RenderTexture ��� PNG */
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
        // ��һ֡����������Ⱦ��UI����������
        yield return new WaitForEndOfFrame();

        // ����Ⱦ/ץͼ
        cam.Render();
        string fileName = $"UnityShot_{System.DateTime.Now:yyyyMMdd_HHmmss}.png";
        string desktopPath = Path.Combine(
            System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop),
            fileName);
        SaveRtToFile(rt, desktopPath);
        Debug.Log($"�ѱ��浽���棺{desktopPath}");
    }
}