using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Order
{
    public Customer customer;
    public ProductData product;
    public int quantity;
    public float bonusMultiplier;
    public float payment;
    public bool isFulfilled;
}

public class OrderManager : MonoBehaviour
{
    public static OrderManager Instance { get; private set; }

    private List<Order> activeOrders = new List<Order>();

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

    public void RegisterOrder(Order order)
    {
        activeOrders.Add(order);
        Debug.Log($"[OrderManager] Order registered. Total active: {activeOrders.Count}");
    }

    /// <summary>
    /// Called by HUDController after the prep progress bar completes.
    /// Immediately fulfills the front order (index 0).
    /// </summary>
    public void FulfillFrontOrder()
    {
        if (activeOrders.Count == 0)
        {
            Debug.LogWarning("[OrderManager] FulfillFrontOrder — no active orders!");
            return;
        }

        Order order = activeOrders[0];
        Debug.Log($"[OrderManager] Fulfilling front order: {order.product.productName}, payment={order.payment:F0} TL, remaining={activeOrders.Count - 1}");

        if (order.customer != null && !order.isFulfilled)
        {
            order.isFulfilled = true;
            order.customer.OnOrderFulfilled(order.payment);
        }

        activeOrders.RemoveAt(0);
    }

    void OnDestroy()
    {
        activeOrders.Clear();
    }

    public float CalculatePayment(Order order)
    {
        return order.quantity * order.product.basePrice * order.bonusMultiplier;
    }

    public bool HasActiveOrder(Customer customer)
    {
        return activeOrders.Exists(o => o.customer == customer && !o.isFulfilled);
    }

    public int GetActiveOrderCount()
    {
        return activeOrders.Count;
    }

    public Order GetFrontOrder()
    {
        if (activeOrders.Count == 0) return null;
        return activeOrders[0];
    }
}
