using System.Collections.Generic;
using UnityEngine;

public class AchievementManager : MonoBehaviour
{
    public static AchievementManager Instance { get; private set; }

    private readonly HashSet<string> unlocked = new HashSet<string>();
    private int servedCount;
    private int preparedCount;
    private bool vipSeen;

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
        servedCount = PlayerPrefs.GetInt("ach_served_count", 0);
        preparedCount = PlayerPrefs.GetInt("ach_prepared_count", 0);
        vipSeen = PlayerPrefs.GetInt("ach_vip_seen", 0) == 1;
        LoadUnlocked();

        GameEvents.OnCustomerServed += OnCustomerServed;
        GameEvents.OnMoneyEarned += OnMoneyEarned;
        GameEvents.OnOrderPrepared += OnOrderPrepared;
        GameEvents.OnWorkerHired += OnWorkerHired;
    }

    void OnDestroy()
    {
        GameEvents.OnCustomerServed -= OnCustomerServed;
        GameEvents.OnMoneyEarned -= OnMoneyEarned;
        GameEvents.OnOrderPrepared -= OnOrderPrepared;
        GameEvents.OnWorkerHired -= OnWorkerHired;
    }

    private void OnCustomerServed(CustomerType type)
    {
        servedCount++;
        PlayerPrefs.SetInt("ach_served_count", servedCount);
        if (servedCount >= 10) Unlock("Acilis Gunu");
        if (type == CustomerType.VIP)
        {
            vipSeen = true;
            PlayerPrefs.SetInt("ach_vip_seen", 1);
            Unlock("VIP Hizmet");
        }
    }

    private void OnMoneyEarned(float amount)
    {
        if (PlayerData.Instance != null && PlayerData.Instance.totalEarned >= 1000f)
            Unlock("Ilk Bin");
    }

    private void OnOrderPrepared()
    {
        preparedCount++;
        PlayerPrefs.SetInt("ach_prepared_count", preparedCount);
        if (preparedCount >= 100) Unlock("Caliskan Kasap");
    }

    private void OnWorkerHired(string type)
    {
        Unlock("Patron");
    }

    private void Unlock(string key)
    {
        if (unlocked.Contains(key)) return;
        unlocked.Add(key);
        SaveUnlocked();
        UIManager.Instance?.ShowToast($"Rozet Acildi: {key}", new Color(0.95f, 0.75f, 0.2f), 2f);
    }

    private void SaveUnlocked()
    {
        string joined = string.Join("|", unlocked);
        PlayerPrefs.SetString("ach_unlocked", joined);
        PlayerPrefs.Save();
    }

    private void LoadUnlocked()
    {
        unlocked.Clear();
        string joined = PlayerPrefs.GetString("ach_unlocked", string.Empty);
        if (string.IsNullOrEmpty(joined)) return;
        string[] items = joined.Split('|');
        for (int i = 0; i < items.Length; i++)
        {
            if (!string.IsNullOrEmpty(items[i])) unlocked.Add(items[i]);
        }
    }
}
