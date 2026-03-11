using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomerManager : MonoBehaviour
{
    public static CustomerManager Instance { get; private set; }

    [SerializeField] private GameObject[] customerPrefabs;
    [SerializeField] private ProductData[] productDataList;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private Transform counterPosition;
    [SerializeField] private Sprite queueArrowSprite;
    [SerializeField] private float baseSpawnInterval = 5f;
    [SerializeField] private int maxCustomers = 5;

    private List<Customer> activeCustomers = new List<Customer>();
    private readonly List<GameObject> queueArrows = new List<GameObject>();
    private Coroutine spawnCoroutine;

    // Queue spacing: index 0 → 0, 1 → 1.2, 2 → -1.2, 3 → 2.4, 4 → -2.4 ...
    private static readonly float QUEUE_SPACING = 1.2f;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Fallback: if prefabs/products not wired via scene, load from RuntimeConfig
        LoadFromRuntimeConfig();
    }

    private void LoadFromRuntimeConfig()
    {
        var config = RuntimeConfig.Instance;
        if (config == null)
        {
            Debug.LogWarning("[CustomerManager] RuntimeConfig not found in Resources!");
            return;
        }

        if (customerPrefabs == null || customerPrefabs.Length == 0)
        {
            customerPrefabs = config.customerPrefabs;
            Debug.Log($"[CustomerManager] Loaded {customerPrefabs?.Length ?? 0} prefab(s) from RuntimeConfig");
        }

        if (productDataList == null || productDataList.Length == 0)
        {
            productDataList = config.productDataList;
            Debug.Log($"[CustomerManager] Loaded {productDataList?.Length ?? 0} product(s) from RuntimeConfig");
        }
    }

    public void StartSpawning()
    {
        Debug.Log($"[CustomerManager] StartSpawning called. Prefabs={customerPrefabs?.Length ?? 0}, Products={productDataList?.Length ?? 0}, SpawnPoint={(spawnPoint != null ? "OK" : "NULL")}, Counter={(counterPosition != null ? "OK" : "NULL")}");
        if (spawnCoroutine != null) StopCoroutine(spawnCoroutine);
        spawnCoroutine = StartCoroutine(SpawnRoutine());
    }

    public void StopSpawning()
    {
        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
            spawnCoroutine = null;
        }
    }

    void OnDestroy()
    {
        StopSpawning();
    }

    private IEnumerator SpawnRoutine()
    {
        while (gameObject.activeSelf)
        {
            float speedMultiplier = ShopManager.Instance != null
                ? ShopManager.Instance.GetSpeedMultiplier()
                : 1f;
            maxCustomers = ShopManager.Instance != null ? ShopManager.Instance.GetMaxCustomers() : maxCustomers;

            float interval = baseSpawnInterval / Mathf.Max(speedMultiplier, 0.1f);
            yield return new WaitForSeconds(interval);

            if (activeCustomers.Count >= maxCustomers)
            {
                Debug.Log($"[CustomerManager] Max customers reached ({maxCustomers}), skipping spawn");
                continue;
            }
            if (customerPrefabs == null || customerPrefabs.Length == 0)
            {
                Debug.LogWarning("[CustomerManager] No customer prefabs assigned!");
                continue;
            }

            Debug.Log($"[CustomerManager] Spawning customer #{activeCustomers.Count + 1}");
            SpawnCustomer();
        }
    }

    private void SpawnCustomer()
    {
        int prefabIndex = Random.Range(0, customerPrefabs.Length);
        Vector3 spawnPos = spawnPoint != null ? spawnPoint.position : transform.position;

        GameObject go = Instantiate(customerPrefabs[prefabIndex], spawnPos, Quaternion.identity);
        Customer customer = go.GetComponent<Customer>();
        if (customer == null)
        {
            Destroy(go);
            return;
        }

        int queueIndex = activeCustomers.Count;
        float offsetX = GetQueueOffsetX(queueIndex);

        CustomerType type = PickCustomerType();
        customer.Initialize(type, counterPosition, offsetX);
        activeCustomers.Add(customer);
        RefreshQueueArrows();
    }

    /// <summary>
    /// Returns X offset for a given queue index.
    /// 0 → 0, 1 → +1.2, 2 → -1.2, 3 → +2.4, 4 → -2.4 ...
    /// </summary>
    public static float GetQueueOffsetX(int index)
    {
        if (index == 0) return 0f;
        int slot = (index + 1) / 2;               // 1,1,2,2,3,3...
        float sign = (index % 2 == 1) ? 1f : -1f; // +, -, +, -, ...
        return slot * QUEUE_SPACING * sign;
    }

    private CustomerType PickCustomerType()
    {
        float total =
            CustomerData.Get(CustomerType.Normal).spawnWeight +
            CustomerData.Get(CustomerType.Aceleci).spawnWeight +
            CustomerData.Get(CustomerType.Sabirli).spawnWeight +
            CustomerData.Get(CustomerType.VIP).spawnWeight;

        float roll = Random.value * total;
        float current = CustomerData.Get(CustomerType.Normal).spawnWeight;
        if (roll < current) return CustomerType.Normal;
        current += CustomerData.Get(CustomerType.Aceleci).spawnWeight;
        if (roll < current) return CustomerType.Aceleci;
        current += CustomerData.Get(CustomerType.Sabirli).spawnWeight;
        if (roll < current) return CustomerType.Sabirli;
        return CustomerType.VIP;
    }

    public void CustomerLeft(Customer customer)
    {
        activeCustomers.Remove(customer);
        ShiftQueue();
        RefreshQueueArrows();
    }

    /// <summary>
    /// After a customer leaves, slide remaining customers to their new queue positions.
    /// </summary>
    private void ShiftQueue()
    {
        Vector3 counterPos = counterPosition != null ? counterPosition.position : Vector3.zero;

        for (int i = 0; i < activeCustomers.Count; i++)
        {
            if (activeCustomers[i] == null) continue;
            float offsetX = GetQueueOffsetX(i);
            Vector3 newTarget = counterPos + new Vector3(offsetX, 0f, 0f);
            activeCustomers[i].SlideToPosition(newTarget);
        }
    }

    private void RefreshQueueArrows()
    {
        for (int i = queueArrows.Count - 1; i >= 0; i--)
        {
            if (queueArrows[i] != null) Destroy(queueArrows[i]);
        }
        queueArrows.Clear();

        if (queueArrowSprite == null || activeCustomers.Count < 2) return;

        for (int i = 0; i < activeCustomers.Count - 1; i++)
        {
            Customer from = activeCustomers[i];
            Customer to = activeCustomers[i + 1];
            if (from == null || to == null) continue;

            Vector3 midpoint = (from.transform.position + to.transform.position) * 0.5f;
            midpoint += new Vector3(0f, -0.35f, 0f);

            GameObject arrow = new GameObject($"QueueArrow_{i}", typeof(SpriteRenderer));
            SpriteRenderer sr = arrow.GetComponent<SpriteRenderer>();
            sr.sprite = queueArrowSprite;
            sr.color = new Color(1f, 1f, 1f, 0.65f);
            sr.sortingOrder = 3;
            arrow.transform.position = midpoint;

            Vector3 dir = (from.transform.position - to.transform.position).normalized;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            arrow.transform.rotation = Quaternion.Euler(0f, 0f, angle);
            arrow.transform.localScale = Vector3.one * 0.7f;

            queueArrows.Add(arrow);
        }
    }

    public int GetActiveCustomerCount()
    {
        return activeCustomers.Count;
    }

    public ProductData[] GetProductDataList()
    {
        return productDataList;
    }
}
