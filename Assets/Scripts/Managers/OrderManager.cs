using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class OrderLine
{
    public ProductData product;
    public int quantity;
    public bool isPrepared;
}

public class Order
{
    public Customer customer;
    public List<OrderLine> lines = new List<OrderLine>();
    public float bonusMultiplier = 1f;
    public float payment;
    public bool isFulfilled;

    public bool IsFullyPrepared()
    {
        for (int i = 0; i < lines.Count; i++)
        {
            if (!lines[i].isPrepared) return false;
        }
        return lines.Count > 0;
    }

    public int GetFirstPendingLine()
    {
        for (int i = 0; i < lines.Count; i++)
        {
            if (!lines[i].isPrepared) return i;
        }
        return -1;
    }
}

public class OrderManager : MonoBehaviour
{
    public static OrderManager Instance { get; private set; }

    private readonly List<Order> activeOrders = new List<Order>();
    private readonly List<PrepStation> stations = new List<PrepStation>();
    private int stationCapacity = 1;

    public event Action OnOrderStateChanged;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        InitializeStations(1);
    }

    void Update()
    {
        int desired = ShopManager.Instance != null ? ShopManager.Instance.GetStationCount() : 1;
        if (desired != stationCapacity)
            InitializeStations(desired);
    }

    private void InitializeStations(int capacity)
    {
        stationCapacity = Mathf.Clamp(capacity, 1, 4);
        while (stations.Count < stationCapacity)
            stations.Add(new PrepStation());
        while (stations.Count > stationCapacity)
            stations.RemoveAt(stations.Count - 1);
    }

    public void RegisterOrder(Order order)
    {
        if (order == null) return;
        order.payment = CalculatePayment(order);
        activeOrders.Add(order);
        OnOrderStateChanged?.Invoke();
    }

    public float CalculatePayment(Order order)
    {
        float sum = 0f;
        for (int i = 0; i < order.lines.Count; i++)
        {
            OrderLine line = order.lines[i];
            if (line.product == null) continue;
            sum += line.product.basePrice * Mathf.Max(1, line.quantity);
        }

        float quality = ShopManager.Instance != null ? ShopManager.Instance.GetQualityMultiplier() : 1f;
        return sum * Mathf.Max(1f, order.bonusMultiplier) * quality;
    }

    public bool TryStartPreparingOrder(int orderIndex)
    {
        if (orderIndex < 0 || orderIndex >= activeOrders.Count) return false;
        PrepStation free = GetFreeStation();
        if (free == null) return false;

        Order order = activeOrders[orderIndex];
        if (order == null || order.isFulfilled) return false;
        int lineIndex = order.GetFirstPendingLine();
        if (lineIndex < 0) return false;

        OrderLine line = order.lines[lineIndex];
        float prepTime = ShopManager.Instance != null
            ? ShopManager.Instance.GetPreparationTime(line.product)
            : (line.product != null ? line.product.GetPrepTime() : 1f);

        free.StartPreparing(this, stations.IndexOf(free), order, lineIndex, prepTime, OnStationDone);
        OnOrderStateChanged?.Invoke();
        return true;
    }

    public bool TryStartPreparingFrontOrder()
    {
        for (int i = 0; i < activeOrders.Count; i++)
        {
            if (TryStartPreparingOrder(i)) return true;
        }
        return false;
    }

    public void TryFulfillAnyReadyOrder(bool includeVip)
    {
        for (int i = 0; i < activeOrders.Count; i++)
        {
            Order o = activeOrders[i];
            if (o == null || o.isFulfilled || !o.IsFullyPrepared() || o.customer == null) continue;
            if (!includeVip && o.customer.Type == CustomerType.VIP) continue;
            FulfillOrderAtIndex(i);
            return;
        }
    }

    public void FulfillFrontReadyOrder()
    {
        for (int i = 0; i < activeOrders.Count; i++)
        {
            if (activeOrders[i] != null && activeOrders[i].IsFullyPrepared())
            {
                FulfillOrderAtIndex(i);
                return;
            }
        }
    }

    private void FulfillOrderAtIndex(int index)
    {
        if (index < 0 || index >= activeOrders.Count) return;
        Order order = activeOrders[index];
        if (order == null || order.isFulfilled) return;
        if (!order.IsFullyPrepared()) return;

        order.isFulfilled = true;
        if (order.customer != null)
            order.customer.OnOrderFulfilled(order.payment);
        activeOrders.RemoveAt(index);
        OnOrderStateChanged?.Invoke();
    }

    private PrepStation GetFreeStation()
    {
        for (int i = 0; i < stations.Count; i++)
        {
            if (!stations[i].isBusy) return stations[i];
        }
        return null;
    }

    private void OnStationDone(PrepStation station)
    {
        if (station == null || station.order == null) return;
        if (station.lineIndex >= 0 && station.lineIndex < station.order.lines.Count)
            station.order.lines[station.lineIndex].isPrepared = true;

        station.order.customer?.OnOrderLinePrepared();
        GameEvents.RaiseOrderPrepared();
        OnOrderStateChanged?.Invoke();
        TryStartPreparingFrontOrder();
    }

    public int GetActiveOrderCount() => activeOrders.Count;

    public int GetReadyOrderCount()
    {
        int c = 0;
        for (int i = 0; i < activeOrders.Count; i++)
        {
            if (activeOrders[i] != null && activeOrders[i].IsFullyPrepared()) c++;
        }
        return c;
    }

    public int GetPreparingCount()
    {
        int c = 0;
        for (int i = 0; i < stations.Count; i++)
            if (stations[i].isBusy) c++;
        return c;
    }

    public float GetFirstStationProgress()
    {
        for (int i = 0; i < stations.Count; i++)
        {
            if (stations[i].isBusy) return stations[i].progress;
        }
        return 0f;
    }

    public string GetFirstStationProductName()
    {
        for (int i = 0; i < stations.Count; i++)
        {
            PrepStation s = stations[i];
            if (!s.isBusy || s.order == null || s.lineIndex < 0 || s.lineIndex >= s.order.lines.Count) continue;
            return s.order.lines[s.lineIndex].product != null ? s.order.lines[s.lineIndex].product.productName : "Urun";
        }
        return string.Empty;
    }

    public Order GetFrontOrder()
    {
        if (activeOrders.Count == 0) return null;
        return activeOrders[0];
    }

    public List<Order> GetActiveOrdersSnapshot()
    {
        return new List<Order>(activeOrders);
    }
}
