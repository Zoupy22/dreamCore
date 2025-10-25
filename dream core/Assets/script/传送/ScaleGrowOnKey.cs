using UnityEngine;

public class ScaleGrowFour_CustomKey : MonoBehaviour
{
    [Header("4 个物体")]
    public Transform[] objs;   // 拖进来

    [Header("每次增加量")]
    public float addScale = 0.2f;

    [Header("动画时长")]
    public float duration = 0.25f;

    [Header("自定义按键——在 Inspector 里点选即可")]
    public KeyCode key = KeyCode.K;   // ← 想改什么键就改这里

    // 内部插值状态
    float targetAdd = 0f;
    float currentAdd = 0f;
    float velocity = 0f;

    void Update()
    {
        // 1. 检测自定义按键
        if (Input.GetKeyDown(key))
        {
            targetAdd += addScale;
        }

        // 2. 平滑插值
        currentAdd = Mathf.SmoothDamp(currentAdd, targetAdd,
                                      ref velocity, duration);

        // 3. 一次性改 4 个物体的 scale.x
        foreach (var t in objs)
        {
            Vector3 s = t.localScale;
            s.x = 1f + currentAdd;   // 若初始不是 1，换成初始值
            t.localScale = s;
        }
    }
}