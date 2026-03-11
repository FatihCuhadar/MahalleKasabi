using System;

public static class GameEvents
{
    public static event Action<float> OnMoneyEarned;
    public static event Action<CustomerType> OnCustomerServed;
    public static event Action OnOrderPrepared;
    public static event Action<string> OnWorkerHired;
    public static event Action<int> OnShopLevelChanged;
    public static event Action<string> OnToastRequested;

    public static void RaiseMoneyEarned(float amount) => OnMoneyEarned?.Invoke(amount);
    public static void RaiseCustomerServed(CustomerType type) => OnCustomerServed?.Invoke(type);
    public static void RaiseOrderPrepared() => OnOrderPrepared?.Invoke();
    public static void RaiseWorkerHired(string workerType) => OnWorkerHired?.Invoke(workerType);
    public static void RaiseShopLevelChanged(int level) => OnShopLevelChanged?.Invoke(level);
    public static void RaiseToast(string message) => OnToastRequested?.Invoke(message);
}
