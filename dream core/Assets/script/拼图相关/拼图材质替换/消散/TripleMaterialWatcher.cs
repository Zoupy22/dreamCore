using System;
using UnityEngine;
using UnityEngine.Events;

public class TripleMaterialWatcher : MonoBehaviour
{
    [Serializable]
    public struct Pair
    {
        public Renderer targetRenderer;   // 玩家拖物体（或拖Renderer）
        public Material requiredMaterial; // 玩家拖材质
    }

    [Header("三对条件")]
    public Pair[] pairs = new Pair[3];   // 长度固定3，Inspector 里拖 3 个即可

    [Header("完成后触发")]
    public UnityEvent onAllMatched;

    private bool[] matched = new bool[3];
    private bool triggered = false;

    /* 公有接口：任何地方换完材质后调用一次即可 */
    public void ReportMatched(Renderer reporter)
    {
        for (int i = 0; i < 3; i++)
        {
            if (reporter == pairs[i].targetRenderer &&
                reporter.material == pairs[i].requiredMaterial)
            {
                matched[i] = true;
                CheckAll();
                return;
            }
        }
    }

    /* 可选：每帧兜底检查（性能无压力） */
    private void LateUpdate()
    {
        if (!triggered) CheckAll();
    }

    private void CheckAll()
    {
        if (triggered) return;

        // ➜ 打印当前 3 个条件状态
        for (int i = 0; i < 3; i++)
        {
            var r = pairs[i].targetRenderer;
            bool ok = r != null && r.material == pairs[i].requiredMaterial;
            Debug.Log($"[{i}]  {r?.name} 材质==要求？ {ok}");
        }

        // 下面是你原来的全判断
        for (int i = 0; i < 3; i++)
        {
            if (pairs[i].targetRenderer == null ||
                pairs[i].targetRenderer.material != pairs[i].requiredMaterial)
                return;
        }

        triggered = true;
        onAllMatched?.Invoke();
        Debug.Log("✅ TripleMaterialWatcher 已触发事件！");
    }
}