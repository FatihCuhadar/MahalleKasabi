using UnityEngine;

/// <summary>
/// Central runtime config loaded via Resources.Load at runtime.
/// Lives at Assets/Resources/RuntimeConfig.asset so it survives scene transitions
/// without relying on Editor-only SerializedObject wiring.
/// </summary>
[CreateAssetMenu(menuName = "MahalleKasabi/RuntimeConfig", fileName = "RuntimeConfig")]
public class RuntimeConfig : ScriptableObject
{
    private static RuntimeConfig _instance;

    public static RuntimeConfig Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = Resources.Load<RuntimeConfig>("RuntimeConfig");
                if (_instance == null)
                    Debug.LogError("[RuntimeConfig] Could not load from Resources/RuntimeConfig!");
            }
            return _instance;
        }
    }

    [Header("Customer")]
    public GameObject[] customerPrefabs;

    [Header("Products")]
    public ProductData[] productDataList;
}
