using System;
using UnityEngine;

public static class SaveManager
{
    private const string KEY_MONEY = "player_money";
    private const string KEY_TOTAL_EARNED = "player_total_earned";
    private const string KEY_SHOP_LEVEL = "shop_level";
    private const string KEY_WORKERS = "workers_json";
    private const string KEY_UPGRADES = "upgrades_json";
    private const string KEY_LAST_SAVE = "last_save_time";

    [Serializable]
    private class WorkerSaveWrapper
    {
        public WorkerData[] workers;
    }

    public static void Save()
    {
        PlayerData data = PlayerData.Instance;
        if (data == null) return;

        PlayerPrefs.SetFloat(KEY_MONEY, data.currentMoney);
        PlayerPrefs.SetFloat(KEY_TOTAL_EARNED, data.totalEarned);
        PlayerPrefs.SetInt(KEY_SHOP_LEVEL, data.shopLevel);

        WorkerSaveWrapper wrapper = new WorkerSaveWrapper { workers = data.workers };
        PlayerPrefs.SetString(KEY_WORKERS, JsonUtility.ToJson(wrapper));

        PlayerPrefs.SetString(KEY_LAST_SAVE, DateTime.UtcNow.ToBinary().ToString());

        PlayerPrefs.Save();
    }

    public static void Load()
    {
        PlayerData data = PlayerData.Instance;
        if (data == null) return;

        data.currentMoney = PlayerPrefs.GetFloat(KEY_MONEY, 100f);
        data.totalEarned = PlayerPrefs.GetFloat(KEY_TOTAL_EARNED, 0f);
        data.shopLevel = PlayerPrefs.GetInt(KEY_SHOP_LEVEL, 1);

        string workersJson = PlayerPrefs.GetString(KEY_WORKERS, "");
        if (!string.IsNullOrEmpty(workersJson))
        {
            WorkerSaveWrapper wrapper = JsonUtility.FromJson<WorkerSaveWrapper>(workersJson);
            data.workers = wrapper != null && wrapper.workers != null ? wrapper.workers : new WorkerData[0];
        }
        else
        {
            data.workers = new WorkerData[0];
        }

        string savedTime = PlayerPrefs.GetString(KEY_LAST_SAVE, "");
        if (!string.IsNullOrEmpty(savedTime) && long.TryParse(savedTime, out long binary))
        {
            try
            {
                data.lastSaveTime = DateTime.FromBinary(binary);
            }
            catch
            {
                data.lastSaveTime = DateTime.UtcNow;
            }
        }
        else
        {
            data.lastSaveTime = DateTime.UtcNow;
        }
    }

    public static void DeleteAll()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
    }
}
