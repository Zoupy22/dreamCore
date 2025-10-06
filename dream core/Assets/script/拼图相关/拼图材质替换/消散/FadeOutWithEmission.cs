using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FadeOutWithEmission : MonoBehaviour
{
    [Header("淡出参数")]
    public float fadeDuration = 1f;
    public bool destroyAfterFade = true;

    private bool fading = false;

    private struct MatInfo
    {
        public Renderer renderer;
        public Material[] materials;
        public Color[] originalColors;
        public Color[] originalEmissions;
    }
    private List<MatInfo> mats = new List<MatInfo>();

    private void Start() => CacheMaterials();

    /* 公有入口：给 TripleMaterialWatcher 调用 */
    public void FadeOut()
    {
        if (fading) return;
        fading = true;
        StartCoroutine(FadeRoutine());
    }

    private IEnumerator FadeRoutine()
    {
        float t = 0;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float ratio = t / fadeDuration;
            float alpha = Mathf.Lerp(1, 0, ratio);
            float emissionMult = alpha;
            ApplyFade(alpha, emissionMult);
            yield return null;
        }
        ApplyFade(0, 0);
        if (destroyAfterFade) Destroy(gameObject);
    }

    private void ApplyFade(float alpha, float emissionMult)
    {
        foreach (var info in mats)
        {
            for (int i = 0; i < info.materials.Length; i++)
            {
                Material m = info.materials[i];
                Color c = info.originalColors[i];
                c.a = alpha;
                if (m.HasProperty("_Color"))
                    m.color = c;
                else if (m.HasProperty("_BaseColor"))
                    m.SetColor("_BaseColor", c);

                Color e = info.originalEmissions[i] * emissionMult;
                if (m.HasProperty("_EmissionColor"))
                    m.SetColor("_EmissionColor", e);
                else if (m.HasProperty("_EmissiveColor"))
                    m.SetColor("_EmissiveColor", e);
            }
        }
    }

    private void CacheMaterials()
    {
        mats.Clear();
        foreach (var ren in GetComponentsInChildren<Renderer>(true))
        {
            var info = new MatInfo
            {
                renderer = ren,
                materials = ren.materials,
                originalColors = new Color[ren.materials.Length],
                originalEmissions = new Color[ren.materials.Length]
            };

            for (int i = 0; i < ren.materials.Length; i++)
            {
                Material m = ren.materials[i];
                Color col = Color.white;
                if (m.HasProperty("_Color")) col = m.color;
                else if (m.HasProperty("_BaseColor")) col = m.GetColor("_BaseColor");
                info.originalColors[i] = col;

                Color em = Color.black;
                if (m.HasProperty("_EmissionColor")) em = m.GetColor("_EmissionColor");
                else if (m.HasProperty("_EmissiveColor")) em = m.GetColor("_EmissiveColor");
                info.originalEmissions[i] = em;
            }
            mats.Add(info);
        }
    }
}