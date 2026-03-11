using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class OfflineEarningsPopup : MonoBehaviour
{
    [SerializeField] private TMP_Text amountText;
    [SerializeField] private TMP_Text timeText;
    [SerializeField] private Button watchAdButton;
    [SerializeField] private Button collectButton;

    private float earnedAmount;

    void Start()
    {
        if (watchAdButton != null)
            watchAdButton.onClick.AddListener(OnWatchAd);

        if (collectButton != null)
            collectButton.onClick.AddListener(OnCollect);
    }

    void OnDestroy()
    {
        if (watchAdButton != null)
            watchAdButton.onClick.RemoveListener(OnWatchAd);

        if (collectButton != null)
            collectButton.onClick.RemoveListener(OnCollect);
    }

    public void Show(float amount, float seconds)
    {
        earnedAmount = amount;
        gameObject.SetActive(true);

        string moneyStr = UIManager.Instance != null
            ? UIManager.Instance.FormatMoney(amount)
            : $"{amount:F0} TL";

        if (amountText != null)
            amountText.text = $"{moneyStr} kazandin!";

        if (timeText != null)
            timeText.text = $"{FormatDuration(seconds)} boyunca dukkanin calisti!";
    }

    private void OnWatchAd()
    {
        AudioManager.Instance?.Play("button_click");

        // AdManager will handle the rewarded ad flow
        // On success, give double earnings
        if (AdManager.Instance != null)
        {
            float bonusAmount = earnedAmount; // extra amount (total becomes 2x)
            AdManager.Instance.ShowRewardedAd(() =>
            {
                PlayerData.Instance?.AddMoney(bonusAmount);
                UIManager.Instance?.ShowToast("2x kazanc alindi!", new Color(0.96f, 0.64f, 0.13f, 1f), 2f);
                gameObject.SetActive(false);
            });
        }
        else
        {
            // No ad manager — just give bonus directly (dev mode)
            PlayerData.Instance?.AddMoney(earnedAmount);
            UIManager.Instance?.ShowToast("2x kazanc alindi!", new Color(0.96f, 0.64f, 0.13f, 1f), 2f);
            gameObject.SetActive(false);
        }
    }

    private void OnCollect()
    {
        AudioManager.Instance?.Play("button_click");
        // Money was already added by OfflineEarningsManager
        gameObject.SetActive(false);
    }

    private string FormatDuration(float totalSeconds)
    {
        int hours = Mathf.FloorToInt(totalSeconds / 3600f);
        int minutes = Mathf.FloorToInt((totalSeconds % 3600f) / 60f);

        if (hours > 0 && minutes > 0)
            return $"{hours} saat {minutes} dakika";
        if (hours > 0)
            return $"{hours} saat";
        if (minutes > 0)
            return $"{minutes} dakika";
        return "1 dakikadan az";
    }
}
