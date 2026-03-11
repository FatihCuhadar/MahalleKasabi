using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class HUDController : MonoBehaviour
{
    [SerializeField] private Image prepProgressBar;
    [SerializeField] private Button counterButton;
    [SerializeField] private Button upgradeButton;
    [SerializeField] private bool debugTapAnywhere;

    private Coroutine prepCoroutine;
    private Color normalBarColor = new Color(0.05f, 0.45f, 0.47f, 1f); // teal #0D7377
    private Color flashColor = new Color(0.96f, 0.64f, 0.13f, 1f);     // gold  #F4A422

    void Start()
    {
        if (counterButton != null)
            counterButton.onClick.AddListener(OnCounterTap);
        else
            Debug.LogWarning("[HUD] counterButton is NULL — not wired in Inspector!");

        if (upgradeButton != null)
            upgradeButton.onClick.AddListener(OnUpgradeTap);

        if (prepProgressBar != null)
            prepProgressBar.fillAmount = 0f;
    }

    void Update()
    {
        if (!debugTapAnywhere) return;

        // Optional debug fallback for quick playtests.
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            OnCounterTap();
        }
        else if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
        {
            OnCounterTap();
        }
    }

    void OnDestroy()
    {
        if (counterButton != null)
            counterButton.onClick.RemoveListener(OnCounterTap);
        if (upgradeButton != null)
            upgradeButton.onClick.RemoveListener(OnUpgradeTap);
    }

    private void OnUpgradeTap()
    {
        AudioManager.Instance?.Play("button_click");
        UIManager.Instance?.ShowUpgradePanel();
    }

    public void OnCounterTap()
    {
        if (OrderManager.Instance == null)
        {
            Debug.LogWarning("[HUD] OnCounterTap — OrderManager.Instance is NULL");
            return;
        }
        if (prepCoroutine != null)
        {
            Debug.Log("[HUD] OnCounterTap — already processing, skipped");
            return;
        }

        int orderCount = OrderManager.Instance.GetActiveOrderCount();
        if (orderCount == 0)
        {
            Debug.Log("[HUD] OnCounterTap — no active orders (0)");
            return;
        }

        Debug.Log($"[HUD] Counter tapped! Active orders: {orderCount}. Starting prep...");
        AudioManager.Instance?.Play("button_click");

        // Get prep time for the front order (index 0)
        float prepTime = 1f;
        Order front = OrderManager.Instance.GetFrontOrder();
        if (ShopManager.Instance != null && front != null)
            prepTime = ShopManager.Instance.GetPreparationTime(front.product);

        prepCoroutine = StartCoroutine(ShowPrepProgress(prepTime));
    }

    private IEnumerator ShowPrepProgress(float duration)
    {
        if (prepProgressBar == null)
        {
            yield return new WaitForSeconds(duration);
            OrderManager.Instance?.FulfillFrontOrder();
            if (counterButton != null) counterButton.interactable = true;
            prepCoroutine = null;
            yield break;
        }

        prepProgressBar.fillAmount = 0f;
        prepProgressBar.color = normalBarColor;
        if (counterButton != null) counterButton.interactable = false;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            if (!gameObject.activeSelf) yield break;
            elapsed += Time.deltaTime;
            prepProgressBar.fillAmount = Mathf.Clamp01(elapsed / duration);
            yield return null;
        }

        prepProgressBar.fillAmount = 1f;

        // Fulfill the front order now that prep is done
        Debug.Log("[HUD] Prep complete — fulfilling front order");
        OrderManager.Instance?.FulfillFrontOrder();

        // Flash effect
        AudioManager.Instance?.Play("order_complete");
        yield return StartCoroutine(FlashBar());
        if (counterButton != null)
        {
            counterButton.interactable = true;
            yield return StartCoroutine(AnimationHelper.PunchScale(counterButton.transform, 0.1f, 0.3f));
        }

        prepProgressBar.fillAmount = 0f;
        prepCoroutine = null;
    }

    private IEnumerator FlashBar()
    {
        float flashDuration = 0.3f;
        float t = 0f;

        while (t < flashDuration)
        {
            if (!gameObject.activeSelf) yield break;
            t += Time.deltaTime;
            float ping = Mathf.PingPong(t * 6f, 1f);
            prepProgressBar.color = Color.Lerp(normalBarColor, flashColor, ping);
            yield return null;
        }

        prepProgressBar.color = normalBarColor;
    }
}
