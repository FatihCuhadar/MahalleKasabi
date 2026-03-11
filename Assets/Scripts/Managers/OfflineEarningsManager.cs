using System;
using UnityEngine;

public class OfflineEarningsManager : MonoBehaviour
{
    public static OfflineEarningsManager Instance { get; private set; }

    private const float MAX_OFFLINE_SECONDS = 28800f; // 8 hours
    private const float MIN_OFFLINE_SECONDS = 30f;

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

    public void CalculateOfflineEarnings()
    {
        if (PlayerData.Instance == null) return;

        double elapsedSeconds = (DateTime.UtcNow - PlayerData.Instance.lastSaveTime).TotalSeconds;

        // Too short, not worth showing
        if (elapsedSeconds < MIN_OFFLINE_SECONDS) return;

        // Cap at max offline duration
        float effectiveSeconds = Mathf.Min((float)elapsedSeconds, MAX_OFFLINE_SECONDS);

        // Get offline multiplier from ShopManager (muhasebeci bonus)
        float offlineMultiplier = ShopManager.Instance != null ? ShopManager.Instance.GetOfflineMultiplier() : 1f;

        float earnings = effectiveSeconds * PlayerData.Instance.moneyPerSecond * offlineMultiplier;
        float minutes = effectiveSeconds / 60f;
        float salary = WorkerManager.Instance != null ? WorkerManager.Instance.CalculateOfflineSalary(minutes) : 0f;
        earnings = Mathf.Max(0f, earnings - salary);

        PlayerData.Instance.AddMoney(earnings);
        UIManager.Instance?.ShowOfflineEarningsPopup(earnings, effectiveSeconds);

        Debug.Log($"[OfflineEarnings] {effectiveSeconds:F0}s offline → +{earnings:F0} money (x{offlineMultiplier} - salary {salary:F0})");
    }
}
