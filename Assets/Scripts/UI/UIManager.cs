using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("HUD")]
    [SerializeField] private TMP_Text moneyText;
    [SerializeField] private TMP_Text shopLevelText;
    [SerializeField] private TMP_Text customerCountText;
    [SerializeField] private RectTransform moneyContainer;

    [Header("Panels")]
    [SerializeField] private GameObject upgradePanel;
    [SerializeField] private GameObject offlineEarningsPopup;
    [SerializeField] private GameObject settingsPanel;

    [Header("Toast")]
    [SerializeField] private TMP_Text toastText;
    [SerializeField] private CanvasGroup toastCanvasGroup;
    [SerializeField] private RectTransform toastRect;
    [SerializeField] private RectTransform floatingRoot;

    private Coroutine toastCoroutine;
    private Coroutine moneyPunchCoroutine;
    private Coroutine panelSlideCoroutine;
    private float toastOriginalY;
    private float previousMoney = -1f;
    private float customerCountRefresh;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        if (toastRect != null)
            toastOriginalY = toastRect.anchoredPosition.y;

        if (toastCanvasGroup != null)
            toastCanvasGroup.alpha = 0f;

        RefreshMoneyUI();
        GameEvents.OnMoneyEarned += ShowMoneyFloating;
        GameEvents.OnShopLevelChanged += OnShopLevelChanged;
    }

    void OnDestroy()
    {
        if (toastCoroutine != null)
            StopCoroutine(toastCoroutine);
        GameEvents.OnMoneyEarned -= ShowMoneyFloating;
        GameEvents.OnShopLevelChanged -= OnShopLevelChanged;
    }

    void Update()
    {
        customerCountRefresh += Time.deltaTime;
        if (customerCountRefresh >= 0.2f)
        {
            customerCountRefresh = 0f;
            if (customerCountText != null)
            {
                int count = CustomerManager.Instance != null ? CustomerManager.Instance.GetActiveCustomerCount() : 0;
                customerCountText.text = "Musteri: " + count;
            }
        }
    }

    // ───────────────────────── Money Display ─────────────────────────

    public void RefreshMoneyUI()
    {
        if (PlayerData.Instance == null) return;

        float money = PlayerData.Instance.currentMoney;
        if (moneyText != null)
            moneyText.text = FormatMoney(money);

        if (shopLevelText != null)
            shopLevelText.text = "SEVIYE " + PlayerData.Instance.shopLevel;

        if (customerCountText != null)
        {
            int count = CustomerManager.Instance != null ? CustomerManager.Instance.GetActiveCustomerCount() : 0;
            customerCountText.text = "Musteri: " + count;
        }

        if (moneyContainer != null && previousMoney >= 0f && money > previousMoney)
        {
            if (moneyPunchCoroutine != null) StopCoroutine(moneyPunchCoroutine);
            moneyPunchCoroutine = StartCoroutine(AnimationHelper.PunchScale(moneyContainer, 0.12f, 0.3f));
        }

        previousMoney = money;
    }

    public string FormatMoney(float amount)
    {
        if (amount >= 1000000f)
            return $"{amount / 1000000f:F1}M TL";
        if (amount >= 1000f)
            return $"{amount / 1000f:F1}K TL";
        return $"{amount:F0} TL";
    }

    // ───────────────────────── Offline Earnings Popup ─────────────────────────

    public void ShowOfflineEarningsPopup(float amount, float seconds)
    {
        if (offlineEarningsPopup == null) return;

        OfflineEarningsPopup popup = offlineEarningsPopup.GetComponent<OfflineEarningsPopup>();
        if (popup != null)
        {
            popup.Show(amount, seconds);
        }
        else
        {
            offlineEarningsPopup.SetActive(true);
        }
    }

    // ───────────────────────── Upgrade Panel ─────────────────────────

    public void ShowUpgradePanel()
    {
        if (upgradePanel == null) return;
        upgradePanel.SetActive(true);

        CanvasGroup cg = upgradePanel.GetComponent<CanvasGroup>();
        if (cg == null) cg = upgradePanel.AddComponent<CanvasGroup>();
        cg.alpha = 0f;

        UpgradePanel panel = upgradePanel.GetComponent<UpgradePanel>();
        panel?.RefreshPanel();

        RectTransform rt = upgradePanel.GetComponent<RectTransform>();
        if (rt != null)
        {
            if (panelSlideCoroutine != null) StopCoroutine(panelSlideCoroutine);
            panelSlideCoroutine = StartCoroutine(ShowUpgradePanelAnimated(rt, cg));
        }
    }

    public void HideUpgradePanel()
    {
        if (upgradePanel != null)
            upgradePanel.SetActive(false);
    }

    // ───────────────────────── Settings Panel ─────────────────────────

    public void ShowSettingsPanel()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(true);
    }

    public void HideSettingsPanel()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(false);
    }

    // ───────────────────────── Toast ─────────────────────────

    public void ShowToast(string message, Color color, float duration = 2f)
    {
        if (toastText == null || toastCanvasGroup == null) return;

        toastText.text = message;
        toastText.color = color;

        if (toastCoroutine != null)
            StopCoroutine(toastCoroutine);

        toastCoroutine = StartCoroutine(ToastRoutine(duration));
    }

    public void ShowToast(string message, float duration = 2f)
    {
        ShowToast(message, Color.white, duration);
    }

    private IEnumerator ToastRoutine(float duration)
    {
        float fadeInTime = 0.3f;
        float fadeOutTime = 0.3f;
        float holdTime = duration - fadeInTime - fadeOutTime;
        if (holdTime < 0f) holdTime = 0f;
        toastCanvasGroup.alpha = 0f;
        Vector2 startPos = new Vector2(toastRect != null ? toastRect.anchoredPosition.x : 0f, toastOriginalY - 20f);
        Vector2 endPos = new Vector2(startPos.x, toastOriginalY);

        if (toastRect != null)
            toastRect.anchoredPosition = startPos;

        yield return StartCoroutine(AnimationHelper.FadeIn(toastCanvasGroup, fadeInTime));
        if (toastRect != null) toastRect.anchoredPosition = endPos;
        yield return new WaitForSecondsRealtime(holdTime);
        yield return StartCoroutine(AnimationHelper.FadeOut(toastCanvasGroup, fadeOutTime));
    }

    private IEnumerator ShowUpgradePanelAnimated(RectTransform panelRect, CanvasGroup cg)
    {
        Vector2 target = Vector2.zero;
        Vector2 from = new Vector2(0f, -220f);
        yield return StartCoroutine(AnimationHelper.SlideIn(panelRect, from, target, 0.35f));
        yield return StartCoroutine(AnimationHelper.FadeIn(cg, 0.2f));
    }

    private void ShowMoneyFloating(float amount)
    {
        if (amount <= 0f) return;
        RectTransform parent = floatingRoot != null ? floatingRoot : transform as RectTransform;
        if (parent == null) return;

        GameObject go = new GameObject("FloatingMoney", typeof(RectTransform), typeof(CanvasGroup), typeof(TextMeshProUGUI), typeof(FloatingText));
        go.transform.SetParent(parent, false);
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(1f, 1f);
        rt.anchorMax = new Vector2(1f, 1f);
        rt.pivot = new Vector2(1f, 1f);
        rt.anchoredPosition = new Vector2(-20f, -120f);
        rt.sizeDelta = new Vector2(240f, 60f);

        TextMeshProUGUI text = go.GetComponent<TextMeshProUGUI>();
        text.alignment = TextAlignmentOptions.Right;
        text.fontSize = 30;
        text.raycastTarget = false;

        FloatingText floating = go.GetComponent<FloatingText>();
        floating.Setup("+" + FormatMoney(amount), new Color(0.2f, 0.9f, 0.35f));
    }

    private void OnShopLevelChanged(int level)
    {
        ShowToast($"SEVIYE ATLADI: {level}", new Color(0.95f, 0.75f, 0.2f), 2.5f);
        if (moneyContainer != null)
            StartCoroutine(AnimationHelper.ShakePosition(moneyContainer, 8f, 0.35f));
    }
}
