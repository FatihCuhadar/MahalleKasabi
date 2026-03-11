using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public static event Action OnGameSaved;
    public static event Action OnGameLoaded;

    public bool isGamePaused { get; private set; }

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
        Debug.Log("[GameManager] Start — Loading game...");
        LoadGame();
        Debug.Log($"[GameManager] Offline earnings check... OfflineEarningsManager={(OfflineEarningsManager.Instance != null ? "OK" : "NULL")}");
        OfflineEarningsManager.Instance?.CalculateOfflineEarnings();
        Debug.Log($"[GameManager] Starting customer spawning... CustomerManager={(CustomerManager.Instance != null ? "OK" : "NULL")}");
        CustomerManager.Instance?.StartSpawning();
    }

    public void SaveGame()
    {
        SaveManager.Save();
        OnGameSaved?.Invoke();
    }

    public void LoadGame()
    {
        SaveManager.Load();
        UIManager.Instance?.RefreshMoneyUI();
        OnGameLoaded?.Invoke();
    }

    public void PauseGame()
    {
        Time.timeScale = 0f;
        isGamePaused = true;
    }

    public void ResumeGame()
    {
        Time.timeScale = 1f;
        isGamePaused = false;
    }

    void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            SaveGame();
        }
    }

    void OnApplicationQuit()
    {
        SaveGame();
    }

    void OnDestroy()
    {
        OnGameSaved = null;
        OnGameLoaded = null;
    }
}
