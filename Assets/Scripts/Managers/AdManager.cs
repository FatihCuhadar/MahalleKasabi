using System;
using UnityEngine;

public class AdManager : MonoBehaviour
{
    public static AdManager Instance { get; private set; }

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

    public void ShowRewardedAd(Action onSuccess)
    {
        // Stub: will be replaced with real AdMob integration
        // For now, simulate success immediately
        Debug.Log("[AdManager] Rewarded ad shown (stub — auto-success)");
        onSuccess?.Invoke();
    }
}
