using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Splash screen: fade in title → hold → fade out → go to MainMenu.
/// </summary>
public class BootLoader : MonoBehaviour
{
    [SerializeField] private CanvasGroup splashGroup;
    [SerializeField] private float fadeInTime = 0.8f;
    [SerializeField] private float holdTime = 1.0f;
    [SerializeField] private float fadeOutTime = 0.7f;

    IEnumerator Start()
    {
        if (splashGroup != null)
            splashGroup.alpha = 0f;

        // Fade in
        yield return StartCoroutine(Fade(0f, 1f, fadeInTime));

        // Hold
        yield return new WaitForSecondsRealtime(holdTime);

        // Fade out
        yield return StartCoroutine(Fade(1f, 0f, fadeOutTime));

        // Go to MainMenu
        SceneManager.LoadScene("MainMenu");
    }

    private IEnumerator Fade(float from, float to, float duration)
    {
        if (splashGroup == null) yield break;

        float t = 0f;
        splashGroup.alpha = from;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            splashGroup.alpha = Mathf.Lerp(from, to, Mathf.Clamp01(t / duration));
            yield return null;
        }
        splashGroup.alpha = to;
    }
}
