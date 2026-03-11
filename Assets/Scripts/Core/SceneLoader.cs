using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Static helper that shows a loading overlay then async-loads the target scene.
/// Attach to a DontDestroyOnLoad GameObject with a Canvas (created by editor setup).
/// </summary>
public class SceneLoader : MonoBehaviour
{
    public static SceneLoader Instance { get; private set; }

    [SerializeField] private CanvasGroup overlayGroup;
    [SerializeField] private Image progressBar;
    [SerializeField] private TMP_Text loadingText;

    private bool isLoading;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Start hidden
        if (overlayGroup != null)
        {
            overlayGroup.alpha = 0f;
            overlayGroup.blocksRaycasts = false;
        }
    }

    /// <summary>
    /// Call from anywhere: SceneLoader.LoadScene("Main");
    /// </summary>
    public static void Load(string sceneName)
    {
        if (Instance != null)
        {
            Instance.LoadSceneInternal(sceneName);
        }
        else
        {
            // Fallback if no instance (e.g. first boot)
            Debug.LogWarning("[SceneLoader] No instance — direct load");
            SceneManager.LoadScene(sceneName);
        }
    }

    private void LoadSceneInternal(string sceneName)
    {
        if (isLoading) return;
        StartCoroutine(LoadRoutine(sceneName));
    }

    private IEnumerator LoadRoutine(string sceneName)
    {
        isLoading = true;

        // Show overlay
        if (overlayGroup != null)
        {
            overlayGroup.blocksRaycasts = true;
            yield return StartCoroutine(FadeCanvasGroup(overlayGroup, 0f, 1f, 0.3f));
        }

        if (progressBar != null) progressBar.fillAmount = 0f;

        // Animated dots on loading text
        Coroutine dotsCoroutine = StartCoroutine(AnimateDots());

        // Async load
        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
        op.allowSceneActivation = false;

        while (op.progress < 0.9f)
        {
            if (progressBar != null)
                progressBar.fillAmount = Mathf.Clamp01(op.progress / 0.9f);
            yield return null;
        }

        // Fill to 100% with small delay for feel
        float fakeProgress = progressBar != null ? progressBar.fillAmount : 0.9f;
        while (fakeProgress < 1f)
        {
            fakeProgress += Time.unscaledDeltaTime * 2f;
            if (progressBar != null)
                progressBar.fillAmount = Mathf.Clamp01(fakeProgress);
            yield return null;
        }

        // Brief hold
        yield return new WaitForSecondsRealtime(0.2f);

        // Activate scene
        op.allowSceneActivation = true;

        // Wait one frame for scene to activate
        yield return null;

        StopCoroutine(dotsCoroutine);

        // Fade out overlay
        if (overlayGroup != null)
        {
            yield return StartCoroutine(FadeCanvasGroup(overlayGroup, 1f, 0f, 0.3f));
            overlayGroup.blocksRaycasts = false;
        }

        isLoading = false;
    }

    private IEnumerator AnimateDots()
    {
        string baseText = "Yukleniyor";
        int dotCount = 0;

        while (true)
        {
            dotCount = (dotCount % 3) + 1;
            string dots = new string('.', dotCount);
            if (loadingText != null)
                loadingText.text = baseText + dots;
            yield return new WaitForSecondsRealtime(0.4f);
        }
    }

    private static IEnumerator FadeCanvasGroup(CanvasGroup cg, float from, float to, float duration)
    {
        float t = 0f;
        cg.alpha = from;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            cg.alpha = Mathf.Lerp(from, to, Mathf.Clamp01(t / duration));
            yield return null;
        }
        cg.alpha = to;
    }
}
