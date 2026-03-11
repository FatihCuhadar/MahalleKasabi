using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;

public class HUDController : MonoBehaviour
{
    [SerializeField] private Image prepProgressBar;
    [SerializeField] private Button counterButton;
    [SerializeField] private Button upgradeButton;
    [SerializeField] private bool debugTapAnywhere;

    private Coroutine prepCoroutine;
    private TMP_Text counterButtonText;
    private Image counterButtonImage;
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

        if (counterButton != null)
        {
            counterButtonText = counterButton.GetComponentInChildren<TMP_Text>();
            counterButtonImage = counterButton.GetComponent<Image>();
        }

        if (OrderManager.Instance != null)
            OrderManager.Instance.OnOrderStateChanged += RefreshButtonState;
        RefreshButtonState();
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
        if (OrderManager.Instance != null)
            OrderManager.Instance.OnOrderStateChanged -= RefreshButtonState;
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

        if (OrderManager.Instance.GetReadyOrderCount() > 0)
        {
            OrderManager.Instance.FulfillFrontReadyOrder();
            StartCoroutine(AnimationHelper.PunchScale(counterButton.transform, 0.1f, 0.3f));
            RefreshButtonState();
            return;
        }

        if (!OrderManager.Instance.TryStartPreparingFrontOrder())
        {
            Debug.Log("[HUD] OnCounterTap — hazirlanacak uygun siparis yok");
            return;
        }
        AudioManager.Instance?.Play("button_click");
        if (prepCoroutine != null) StopCoroutine(prepCoroutine);
        prepCoroutine = StartCoroutine(TrackFirstStationProgress());
        RefreshButtonState();
    }

    private IEnumerator TrackFirstStationProgress()
    {
        while (OrderManager.Instance != null && OrderManager.Instance.GetPreparingCount() > 0)
        {
            if (prepProgressBar != null)
            {
                prepProgressBar.fillAmount = OrderManager.Instance.GetFirstStationProgress();
                prepProgressBar.color = normalBarColor;
            }
            SetPreparingVisual();
            yield return null;
        }

        if (prepProgressBar != null) prepProgressBar.fillAmount = 0f;
        prepCoroutine = null;
        RefreshButtonState();
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

    private void RefreshButtonState()
    {
        if (OrderManager.Instance == null || counterButton == null) return;

        bool hasReady = OrderManager.Instance.GetReadyOrderCount() > 0;
        bool hasPreparing = OrderManager.Instance.GetPreparingCount() > 0;
        counterButton.interactable = hasReady || hasPreparing || OrderManager.Instance.GetActiveOrderCount() > 0;

        if (hasReady)
        {
            if (counterButtonText != null) counterButtonText.text = "TESLIM ET!";
            if (counterButtonImage != null) counterButtonImage.color = new Color(0.22f, 0.72f, 0.31f, 0.92f);
        }
        else if (hasPreparing)
        {
            SetPreparingVisual();
        }
        else
        {
            if (counterButtonText != null) counterButtonText.text = "SIPARIS VER";
            if (counterButtonImage != null) counterButtonImage.color = new Color(0.91f, 0.47f, 0.17f, 0.8f);
        }
    }

    private void SetPreparingVisual()
    {
        if (counterButtonText != null) counterButtonText.text = "Hazirlaniyor...";
        if (counterButtonImage != null) counterButtonImage.color = new Color(0.82f, 0.18f, 0.18f, 0.9f);
    }
}
