using System;
using System.Collections.Generic;
using UnityEngine;

public class DailyQuestManager : MonoBehaviour
{
    [Serializable]
    public class DailyQuest
    {
        public string id;
        public string title;
        public int target;
        public int progress;
        public float rewardMoney;
        public bool completed;
    }

    public static DailyQuestManager Instance { get; private set; }
    public IReadOnlyList<DailyQuest> Quests => quests;

    private readonly List<DailyQuest> quests = new List<DailyQuest>();
    private string dateKey;

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
        dateKey = DateTime.UtcNow.ToString("yyyyMMdd");
        GenerateIfNeeded();
        GameEvents.OnCustomerServed += OnCustomerServed;
        GameEvents.OnMoneyEarned += OnMoneyEarned;
        GameEvents.OnOrderPrepared += OnOrderPrepared;
    }

    void OnDestroy()
    {
        GameEvents.OnCustomerServed -= OnCustomerServed;
        GameEvents.OnMoneyEarned -= OnMoneyEarned;
        GameEvents.OnOrderPrepared -= OnOrderPrepared;
    }

    private void GenerateIfNeeded()
    {
        string savedDate = PlayerPrefs.GetString("daily_quests_date", string.Empty);
        if (savedDate == dateKey)
        {
            Load();
            return;
        }

        quests.Clear();
        quests.Add(new DailyQuest { id = "serve_customers", title = "10 musteri karsila", target = 10, rewardMoney = 500f });
        quests.Add(new DailyQuest { id = "earn_money", title = "2000 TL kazan", target = 2000, rewardMoney = 700f });
        quests.Add(new DailyQuest { id = "prepare_orders", title = "20 siparis hazirla", target = 20, rewardMoney = 900f });
        Save();
    }

    private void OnCustomerServed(CustomerType type)
    {
        AddProgress("serve_customers", 1);
        if (type == CustomerType.VIP) AddProgress("serve_vip", 1);
    }

    private void OnMoneyEarned(float amount)
    {
        AddProgress("earn_money", Mathf.RoundToInt(amount));
    }

    private void OnOrderPrepared()
    {
        AddProgress("prepare_orders", 1);
    }

    private void AddProgress(string id, int amount)
    {
        for (int i = 0; i < quests.Count; i++)
        {
            DailyQuest q = quests[i];
            if (q.id != id || q.completed) continue;
            q.progress += amount;
            if (q.progress >= q.target)
            {
                q.completed = true;
                PlayerData.Instance?.AddMoney(q.rewardMoney);
                UIManager.Instance?.ShowToast($"Gorev tamamlandi: {q.title}", new Color(0.2f, 0.8f, 0.3f), 2f);
            }
            Save();
            return;
        }
    }

    private void Save()
    {
        PlayerPrefs.SetString("daily_quests_date", dateKey);
        PlayerPrefs.SetInt("daily_quest_count", quests.Count);
        for (int i = 0; i < quests.Count; i++)
        {
            string p = $"daily_quest_{i}_";
            PlayerPrefs.SetString(p + "id", quests[i].id);
            PlayerPrefs.SetString(p + "title", quests[i].title);
            PlayerPrefs.SetInt(p + "target", quests[i].target);
            PlayerPrefs.SetInt(p + "progress", quests[i].progress);
            PlayerPrefs.SetFloat(p + "reward", quests[i].rewardMoney);
            PlayerPrefs.SetInt(p + "completed", quests[i].completed ? 1 : 0);
        }
        PlayerPrefs.Save();
    }

    private void Load()
    {
        quests.Clear();
        int count = PlayerPrefs.GetInt("daily_quest_count", 0);
        for (int i = 0; i < count; i++)
        {
            string p = $"daily_quest_{i}_";
            quests.Add(new DailyQuest
            {
                id = PlayerPrefs.GetString(p + "id", string.Empty),
                title = PlayerPrefs.GetString(p + "title", string.Empty),
                target = PlayerPrefs.GetInt(p + "target", 1),
                progress = PlayerPrefs.GetInt(p + "progress", 0),
                rewardMoney = PlayerPrefs.GetFloat(p + "reward", 0f),
                completed = PlayerPrefs.GetInt(p + "completed", 0) == 1
            });
        }
    }
}
