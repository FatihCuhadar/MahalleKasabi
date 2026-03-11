using System.Collections;
using TMPro;
using UnityEngine;

public class FloatingText : MonoBehaviour
{
    [SerializeField] private TMP_Text label;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private float duration = 1.2f;
    [SerializeField] private float riseAmount = 80f;

    public void Setup(string message, Color color)
    {
        if (label != null)
        {
            label.text = message;
            label.color = color;
        }
        StartCoroutine(Play());
    }

    private IEnumerator Play()
    {
        RectTransform rt = transform as RectTransform;
        Vector2 start = rt != null ? rt.anchoredPosition : Vector2.zero;
        Vector2 end = start + Vector2.up * riseAmount;
        float t = 0f;

        if (canvasGroup != null) canvasGroup.alpha = 1f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float p = Mathf.Clamp01(t / duration);
            if (rt != null) rt.anchoredPosition = Vector2.Lerp(start, end, p);
            if (canvasGroup != null) canvasGroup.alpha = 1f - p;
            yield return null;
        }

        Destroy(gameObject);
    }
}
