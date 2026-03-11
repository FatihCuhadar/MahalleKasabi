using System.Collections.Generic;
using UnityEngine;

public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance { get; private set; }

    private readonly Dictionary<string, int> levels = new Dictionary<string, int>();
    private readonly Dictionary<string, float[]> costs = new Dictionary<string, float[]>
    {
        { "prep_speed", new[] { 500f, 1200f, 2500f, 5000f, 10000f } },
        { "extra_station", new[] { 2000f, 8000f, 20000f } },
        { "quality", new[] { 800f, 2000f, 4500f, 9000f, 18000f } },
        { "capacity", new[] { 1500f, 4000f, 8000f, 15000f, 30000f } },
        { "menu", new[] { 3000f, 8000f, 20000f, 50000f } },
        { "vitrin", new[] { 2500f, 10000f, 30000f } }
    };

    private readonly Dictionary<string, float> workerHireCosts = new Dictionary<string, float>
    {
        { "cirak", 5000f },
        { "yardimci", 15000f },
        { "usta_kasap", 40000f }
    };

    private readonly Dictionary<string, float> workerMinuteSalary = new Dictionary<string, float>
    {
        { "cirak", 100f },
        { "yardimci", 300f },
        { "usta_kasap", 800f }
    };

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        EnsureDefaults();
    }

    private void EnsureDefaults()
    {
        string[] keys = { "prep_speed", "extra_station", "quality", "capacity", "menu", "vitrin" };
        for (int i = 0; i < keys.Length; i++)
        {
            if (!levels.ContainsKey(keys[i])) levels[keys[i]] = PlayerPrefs.GetInt("upg_" + keys[i], 0);
        }
    }

    public bool TryUpgrade(string key)
    {
        if (!costs.ContainsKey(key) || PlayerData.Instance == null) return false;
        int lvl = GetLevel(key);
        float[] arr = costs[key];
        if (lvl >= arr.Length) return false;
        float price = arr[lvl];
        if (!PlayerData.Instance.SpendMoney(price)) return false;

        levels[key] = lvl + 1;
        PlayerPrefs.SetInt("upg_" + key, levels[key]);
        PlayerPrefs.Save();
        UIManager.Instance?.RefreshMoneyUI();
        return true;
    }

    public int GetLevel(string key)
    {
        EnsureDefaults();
        return levels.ContainsKey(key) ? levels[key] : 0;
    }

    public float GetUpgradeCost(string key)
    {
        if (!costs.ContainsKey(key)) return 0f;
        int lvl = GetLevel(key);
        float[] arr = costs[key];
        if (lvl >= arr.Length) return 0f;
        return arr[lvl];
    }

    public bool IsMaxed(string key)
    {
        if (!costs.ContainsKey(key)) return true;
        return GetLevel(key) >= costs[key].Length;
    }

    public float GetPreparationTime(ProductData product)
    {
        float baseTime = product != null ? product.GetPrepTime() : 1f;
        float speed = 1f + (GetLevel("prep_speed") * 0.2f);
        return baseTime / Mathf.Max(0.1f, speed);
    }

    public int GetStationCount()
    {
        return 1 + GetLevel("extra_station");
    }

    public float GetQualityMultiplier()
    {
        return 1f + (GetLevel("quality") * 0.15f);
    }

    public int GetMaxCustomers()
    {
        return Mathf.Clamp(3 + GetLevel("capacity"), 3, 8);
    }

    public float GetSpeedMultiplier()
    {
        return 1f + (GetLevel("vitrin") * 0.25f);
    }

    public int GetMenuLevel()
    {
        return 1 + GetLevel("menu");
    }

    public bool IsProductUnlocked(ProductData product)
    {
        if (product == null) return false;
        return product.unlockLevel <= GetMenuLevel() + 1;
    }

    public bool TryHireWorker(string workerType)
    {
        if (PlayerData.Instance == null || !workerHireCosts.ContainsKey(workerType)) return false;
        if (IsWorkerHired(workerType)) return false;

        float price = workerHireCosts[workerType];
        if (!PlayerData.Instance.SpendMoney(price)) return false;

        List<WorkerData> list = new List<WorkerData>(PlayerData.Instance.workers ?? new WorkerData[0]);
        list.Add(new WorkerData(workerType, 1, true));
        PlayerData.Instance.workers = list.ToArray();
        WorkerManager.Instance?.RefreshWorkers();
        GameEvents.RaiseWorkerHired(workerType);
        return true;
    }

    public bool IsWorkerHired(string workerType)
    {
        if (PlayerData.Instance == null || PlayerData.Instance.workers == null) return false;
        for (int i = 0; i < PlayerData.Instance.workers.Length; i++)
        {
            WorkerData w = PlayerData.Instance.workers[i];
            if (w != null && w.workerType == workerType && w.isUnlocked) return true;
        }
        return false;
    }

    public float GetWorkerSalaryPerMinute(string workerType)
    {
        return workerMinuteSalary.ContainsKey(workerType) ? workerMinuteSalary[workerType] : 0f;
    }

    public float GetOfflineMultiplier()
    {
        float workerBoost = WorkerManager.Instance != null ? WorkerManager.Instance.GetOfflineWorkerMultiplier() : 1f;
        return workerBoost;
    }
}
