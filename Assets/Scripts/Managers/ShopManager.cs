using System.Collections.Generic;
using UnityEngine;

public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance { get; private set; }

    [SerializeField] private ShopUpgradeData[] upgrades;
    [SerializeField] private WorkerConfigData[] workerConfigs;

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

    // ───────────────────────── Shop Upgrade ─────────────────────────

    public bool TryUpgradeShop()
    {
        if (PlayerData.Instance == null) return false;

        int currentLevel = PlayerData.Instance.shopLevel;
        if (currentLevel >= 10) return false;

        float cost = GetUpgradeCost();
        if (!PlayerData.Instance.SpendMoney(cost)) return false;

        PlayerData.Instance.shopLevel++;

        // Update moneyPerSecond from upgrade data
        ShopUpgradeData nextData = GetUpgradeData(PlayerData.Instance.shopLevel);
        if (nextData != null)
            PlayerData.Instance.moneyPerSecond = nextData.moneyPerSecond;

        AudioManager.Instance?.Play("level_up");
        UIManager.Instance?.RefreshMoneyUI();

        Debug.Log($"[ShopManager] Shop upgraded to level {PlayerData.Instance.shopLevel}");
        return true;
    }

    public float GetUpgradeCost()
    {
        if (PlayerData.Instance == null) return 0f;
        int nextLevel = PlayerData.Instance.shopLevel + 1;
        ShopUpgradeData data = GetUpgradeData(nextLevel);
        return data != null ? data.cost : 0f;
    }

    public ShopUpgradeData GetCurrentUpgradeData()
    {
        if (PlayerData.Instance == null) return null;
        return GetUpgradeData(PlayerData.Instance.shopLevel);
    }

    private ShopUpgradeData GetUpgradeData(int level)
    {
        if (upgrades == null) return null;
        foreach (var u in upgrades)
        {
            if (u.level == level) return u;
        }
        return null;
    }

    // ───────────────────────── Preparation Time ─────────────────────────

    public float GetPreparationTime(ProductData product)
    {
        if (product == null) return 1f;
        float multiplier = GetWorkerSpeedMultiplier();
        return product.preparationTime / Mathf.Max(multiplier, 0.1f);
    }

    private float GetWorkerSpeedMultiplier()
    {
        return GetWorkerBonusValue("kasap_yardimcisi", 1f);
    }

    // ───────────────────────── Speed Multiplier (Kasiyer) ─────────────────────────

    public float GetSpeedMultiplier()
    {
        return GetWorkerBonusValue("kasiyer", 1f);
    }

    // ───────────────────────── Offline Multiplier (Muhasebeci) ─────────────────────────

    public float GetOfflineMultiplier()
    {
        return GetWorkerBonusValue("muhasebeci", 1f);
    }

    // ───────────────────────── Toplu Siparis Bonus (Teslimatci) ─────────────────────────

    public float GetBulkOrderBonus()
    {
        return GetWorkerBonusValue("teslimatci", 0f);
    }

    // ───────────────────────── Worker Hire / Upgrade ─────────────────────────

    public bool TryHireWorker(string workerType)
    {
        if (PlayerData.Instance == null) return false;

        // Already hired?
        if (IsWorkerHired(workerType)) return false;

        WorkerConfigData config = GetWorkerConfig(workerType);
        if (config == null) return false;

        // Check shop level requirement
        if (PlayerData.Instance.shopLevel < config.unlockAtShopLevel) return false;

        // Check cost
        if (!PlayerData.Instance.SpendMoney(config.baseCost)) return false;

        // Add worker to PlayerData
        WorkerData newWorker = new WorkerData(workerType, 1, true);
        var workerList = new List<WorkerData>(PlayerData.Instance.workers);
        workerList.Add(newWorker);
        PlayerData.Instance.workers = workerList.ToArray();

        AudioManager.Instance?.Play("upgrade_success");
        Debug.Log($"[ShopManager] Worker hired: {config.displayName}");
        return true;
    }

    public bool IsWorkerHired(string workerType)
    {
        WorkerData worker = FindWorker(workerType);
        return worker != null && worker.isUnlocked;
    }

    public bool TryUpgradeWorker(string workerType)
    {
        if (PlayerData.Instance == null) return false;

        WorkerData worker = FindWorker(workerType);
        if (worker == null || !worker.isUnlocked) return false;

        WorkerConfigData config = GetWorkerConfig(workerType);
        if (config == null) return false;

        // Max level is bonusValues.Length (5)
        if (worker.level >= config.bonusValues.Length) return false;

        float cost = GetWorkerUpgradeCost(workerType);
        if (!PlayerData.Instance.SpendMoney(cost)) return false;

        worker.level++;

        AudioManager.Instance?.Play("upgrade_success");
        Debug.Log($"[ShopManager] Worker upgraded: {config.displayName} → Level {worker.level}");
        return true;
    }

    public float GetWorkerUpgradeCost(string workerType)
    {
        WorkerData worker = FindWorker(workerType);
        if (worker == null || !worker.isUnlocked) return 0f;

        WorkerConfigData config = GetWorkerConfig(workerType);
        if (config == null || config.upgradeCosts == null) return 0f;

        // upgradeCosts index: level 1→2 is index 0, level 2→3 is index 1, etc.
        int upgradeIndex = worker.level - 1;
        if (upgradeIndex < 0 || upgradeIndex >= config.upgradeCosts.Length) return 0f;

        return config.upgradeCosts[upgradeIndex];
    }

    public int GetWorkerLevel(string workerType)
    {
        WorkerData worker = FindWorker(workerType);
        if (worker == null || !worker.isUnlocked) return 0;
        return worker.level;
    }

    // ───────────────────────── Internal Helpers ─────────────────────────

    private float GetWorkerBonusValue(string workerType, float defaultValue)
    {
        WorkerData worker = FindWorker(workerType);
        if (worker == null || !worker.isUnlocked) return defaultValue;

        WorkerConfigData config = GetWorkerConfig(workerType);
        if (config == null || config.bonusValues == null || config.bonusValues.Length == 0)
            return defaultValue;

        int index = Mathf.Clamp(worker.level - 1, 0, config.bonusValues.Length - 1);
        return config.bonusValues[index];
    }

    private WorkerData FindWorker(string workerType)
    {
        if (PlayerData.Instance == null || PlayerData.Instance.workers == null) return null;
        foreach (var w in PlayerData.Instance.workers)
        {
            if (w.workerType == workerType) return w;
        }
        return null;
    }

    private WorkerConfigData GetWorkerConfig(string workerType)
    {
        if (workerConfigs == null) return null;
        foreach (var c in workerConfigs)
        {
            if (c.workerType == workerType) return c;
        }
        return null;
    }
}
