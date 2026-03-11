using System;
using UnityEngine;

public class PlayerData : MonoBehaviour
{
    public static PlayerData Instance { get; private set; }

    public float currentMoney = 100f;
    public float totalEarned;
    public int shopLevel = 1;
    public float moneyPerSecond = 1f;
    public WorkerData[] workers = new WorkerData[0];
    public DateTime lastSaveTime;

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

    public bool CanAfford(float amount)
    {
        return currentMoney >= amount;
    }

    public void AddMoney(float amount)
    {
        currentMoney += amount;
        totalEarned += amount;
        Debug.Log($"[PlayerData] Money added: +{amount:F0} TL → Total: {currentMoney:F0} TL");
        GameEvents.RaiseMoneyEarned(amount);
        UIManager.Instance?.RefreshMoneyUI();
    }

    public bool SpendMoney(float amount)
    {
        if (!CanAfford(amount)) return false;
        currentMoney -= amount;
        UIManager.Instance?.RefreshMoneyUI();
        return true;
    }
}
