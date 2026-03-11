using System.Collections;
using UnityEngine;

public static class AnimationHelper
{
    public static IEnumerator PunchScale(Transform t, float amount, float duration)
    {
        if (t == null || duration <= 0f) yield break;

        Vector3 original = t.localScale;
        Vector3 punch = original * (1f + amount);
        float half = duration * 0.5f;
        float elapsed = 0f;

        while (elapsed < half)
        {
            elapsed += Time.unscaledDeltaTime;
            float p = Mathf.Clamp01(elapsed / half);
            t.localScale = Vector3.LerpUnclamped(original, punch, EaseOutBack(p));
            yield return null;
        }

        elapsed = 0f;
        while (elapsed < half)
        {
            elapsed += Time.unscaledDeltaTime;
            float p = Mathf.Clamp01(elapsed / half);
            t.localScale = Vector3.Lerp(punch, original, p);
            yield return null;
        }

        t.localScale = original;
    }

    public static IEnumerator FadeIn(CanvasGroup cg, float duration)
    {
        if (cg == null) yield break;
        if (duration <= 0f)
        {
            cg.alpha = 1f;
            yield break;
        }

        float elapsed = 0f;
        float start = cg.alpha;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            cg.alpha = Mathf.Lerp(start, 1f, Mathf.Clamp01(elapsed / duration));
            yield return null;
        }
        cg.alpha = 1f;
    }

    public static IEnumerator FadeOut(CanvasGroup cg, float duration)
    {
        if (cg == null) yield break;
        if (duration <= 0f)
        {
            cg.alpha = 0f;
            yield break;
        }

        float elapsed = 0f;
        float start = cg.alpha;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            cg.alpha = Mathf.Lerp(start, 0f, Mathf.Clamp01(elapsed / duration));
            yield return null;
        }
        cg.alpha = 0f;
    }

    public static IEnumerator SlideIn(RectTransform rt, Vector2 from, Vector2 to, float duration)
    {
        if (rt == null) yield break;
        rt.anchoredPosition = from;
        if (duration <= 0f)
        {
            rt.anchoredPosition = to;
            yield break;
        }

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float p = Mathf.Clamp01(elapsed / duration);
            float eased = EaseOutCubic(p);
            rt.anchoredPosition = Vector2.LerpUnclamped(from, to, eased);
            yield return null;
        }

        rt.anchoredPosition = to;
    }

    public static IEnumerator ShakePosition(Transform t, float intensity, float duration)
    {
        if (t == null || duration <= 0f) yield break;

        Vector3 original = t.localPosition;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float damper = 1f - Mathf.Clamp01(elapsed / duration);
            float x = Random.Range(-1f, 1f) * intensity * damper;
            float y = Random.Range(-1f, 1f) * intensity * damper;
            t.localPosition = original + new Vector3(x, y, 0f);
            yield return null;
        }

        t.localPosition = original;
    }

    private static float EaseOutCubic(float t)
    {
        float x = 1f - t;
        return 1f - x * x * x;
    }

    private static float EaseOutBack(float t)
    {
        const float c1 = 1.70158f;
        const float c3 = c1 + 1f;
        return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
    }
}
