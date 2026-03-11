using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorkerManager : MonoBehaviour
{
    public static WorkerManager Instance { get; private set; }

    private readonly Dictionary<string, Coroutine> activeRoutines = new Dictionary<string, Coroutine>();
    private System.Action<string> workerHiredHandler;

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
        RefreshWorkers();
        workerHiredHandler = _ => RefreshWorkers();
        GameEvents.OnWorkerHired += workerHiredHandler;
    }

    void OnDestroy()
    {
        if (workerHiredHandler != null)
            GameEvents.OnWorkerHired -= workerHiredHandler;
    }

    public void RefreshWorkers()
    {
        StopAll();
        if (PlayerData.Instance == null || PlayerData.Instance.workers == null) return;

        foreach (var worker in PlayerData.Instance.workers)
        {
            if (worker == null || !worker.isUnlocked) continue;
            float interval = GetInterval(worker.workerType);
            if (interval <= 0f) continue;
            activeRoutines[worker.workerType] = StartCoroutine(AutoServeRoutine(worker.workerType, interval));
        }
    }

    private IEnumerator AutoServeRoutine(string workerType, float interval)
    {
        while (gameObject.activeSelf)
        {
            yield return new WaitForSeconds(interval);
            if (OrderManager.Instance == null) continue;
            OrderManager.Instance.TryFulfillAnyReadyOrder(workerType == "usta_kasap");
        }
    }

    public float GetOfflineWorkerMultiplier()
    {
        float value = 1f;
        if (PlayerData.Instance == null || PlayerData.Instance.workers == null) return value;

        foreach (var worker in PlayerData.Instance.workers)
        {
            if (worker == null || !worker.isUnlocked) continue;
            if (worker.workerType == "cirak") value += 0.1f;
            else if (worker.workerType == "yardimci") value += 0.2f;
            else if (worker.workerType == "usta_kasap") value += 0.35f;
        }
        return value;
    }

    public float CalculateOfflineSalary(float minutes)
    {
        float salary = 0f;
        if (PlayerData.Instance == null || PlayerData.Instance.workers == null) return salary;
        foreach (var worker in PlayerData.Instance.workers)
        {
            if (worker == null || !worker.isUnlocked) continue;
            if (worker.workerType == "cirak") salary += 100f * minutes;
            else if (worker.workerType == "yardimci") salary += 300f * minutes;
            else if (worker.workerType == "usta_kasap") salary += 800f * minutes;
        }
        return salary;
    }

    private float GetInterval(string workerType)
    {
        if (workerType == "cirak") return 4f;
        if (workerType == "yardimci") return 2.5f;
        if (workerType == "usta_kasap") return 1.5f;
        return 0f;
    }

    private void StopAll()
    {
        foreach (var kv in activeRoutines)
        {
            if (kv.Value != null) StopCoroutine(kv.Value);
        }
        activeRoutines.Clear();
    }
}
