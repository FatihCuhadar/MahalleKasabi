using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Customer : MonoBehaviour
{
    [SerializeField] private CustomerType customerType;
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private Animator animator;
    [SerializeField] private GameObject orderBubblePrefab;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite happySprite;
    [SerializeField] private Sprite angrySprite;

    private float patienceTime;
    private float bonusMultiplier;
    private int orderQuantity;
    private Transform counterPosition;
    private float counterOffsetX;
    private CustomerOrderBubble bubble;
    private Coroutine patienceCoroutine;
    private Coroutine slideCoroutine;
    private bool orderFulfilled;

    // Animator parameter names
    private static readonly int AnimWalk = Animator.StringToHash("Walk");
    private static readonly string AnimIdle = "Idle";
    private static readonly string AnimHappy = "Happy";
    private static readonly string AnimAngry = "Angry";

    public CustomerType Type => customerType;
    public float BonusMultiplier => bonusMultiplier;
    public int OrderQuantity => orderQuantity;

    public void Initialize(CustomerType type, Transform counter, float offsetX = 0f)
    {
        customerType = type;
        counterPosition = counter;
        counterOffsetX = offsetX;

        switch (type)
        {
            case CustomerType.Normal:
                patienceTime = 30f;
                bonusMultiplier = 1f;
                orderQuantity = 1;
                break;
            case CustomerType.Aceleci:
                patienceTime = 10f;
                bonusMultiplier = 1.5f;
                orderQuantity = 1;
                break;
            case CustomerType.TopluSiparis:
                patienceTime = 20f;
                bonusMultiplier = 3f;
                orderQuantity = 3;
                break;
            case CustomerType.VIP:
                patienceTime = 60f;
                bonusMultiplier = 2f;
                orderQuantity = 1;
                break;
        }

        Debug.Log($"[Customer] Initialized: type={type}, offsetX={offsetX:F1}, counter={(counterPosition != null ? counterPosition.position.ToString() : "NULL")}");
        StartCoroutine(MoveToCounter());
    }

    private IEnumerator MoveToCounter()
    {
        if (animator != null) animator.SetBool(AnimWalk, true);

        Vector3 basePos = counterPosition != null
            ? counterPosition.position
            : transform.position + Vector3.left * 3f;
        Vector3 target = basePos + new Vector3(counterOffsetX, 0f, 0f);

        while (Vector3.Distance(transform.position, target) > 0.05f)
        {
            if (!gameObject.activeSelf) yield break;
            transform.position = Vector3.MoveTowards(
                transform.position, target, moveSpeed * Time.deltaTime);
            yield return null;
        }

        transform.position = target;
        if (animator != null)
        {
            animator.SetBool(AnimWalk, false);
            animator.SetTrigger(AnimIdle);
        }

        Debug.Log($"[Customer] Arrived at counter ({target}). Showing order...");
        ShowOrder();
    }

    private void ShowOrder()
    {
        // Pick a random unlocked product
        ProductData product = GetRandomProduct();
        if (product == null)
        {
            Debug.LogWarning($"[Customer] {customerType} — GetRandomProduct returned NULL! No products available.");
            return;
        }

        // Register order with OrderManager
        Order order = new Order
        {
            customer = this,
            product = product,
            quantity = orderQuantity,
            bonusMultiplier = bonusMultiplier,
            isFulfilled = false
        };
        order.payment = OrderManager.Instance != null
            ? OrderManager.Instance.CalculatePayment(order)
            : product.basePrice * orderQuantity * bonusMultiplier;

        if (OrderManager.Instance != null)
        {
            OrderManager.Instance.RegisterOrder(order);
            Debug.Log($"[Customer] Order registered: {product.productName} x{orderQuantity}, payment={order.payment:F0} TL");
        }
        else
        {
            Debug.LogWarning("[Customer] OrderManager.Instance is NULL — cannot register order!");
        }

        // Show bubble
        if (orderBubblePrefab != null)
        {
            GameObject bubbleGO = Instantiate(orderBubblePrefab);
            bubble = bubbleGO.GetComponent<CustomerOrderBubble>();
            bubble?.AttachTo(transform, new Vector3(0f, 1.8f, 0f));
            bubble?.Show(product, orderQuantity);
        }
        else
        {
            Debug.LogWarning("[Customer] orderBubblePrefab is NULL — no bubble shown");
        }

        // Start patience countdown
        patienceCoroutine = StartCoroutine(PatienceCountdown());
    }

    private ProductData GetRandomProduct()
    {
        ProductData[] allProducts = CustomerManager.Instance != null
            ? CustomerManager.Instance.GetProductDataList()
            : null;

        if (allProducts == null || allProducts.Length == 0)
        {
            Debug.LogWarning($"[Customer] ProductDataList is {(allProducts == null ? "NULL" : "EMPTY")}! CustomerManager.Instance={(CustomerManager.Instance != null ? "OK" : "NULL")}");
            return null;
        }

        int shopLevel = PlayerData.Instance != null ? PlayerData.Instance.shopLevel : 1;
        var unlocked = new System.Collections.Generic.List<ProductData>();
        foreach (var p in allProducts)
        {
            if (p != null && p.unlockLevel <= shopLevel) unlocked.Add(p);
        }
        if (unlocked.Count == 0) return allProducts[0];
        return unlocked[Random.Range(0, unlocked.Count)];
    }

    private IEnumerator PatienceCountdown()
    {
        float elapsed = 0f;

        while (elapsed < patienceTime)
        {
            if (!gameObject.activeSelf) yield break;

            elapsed += Time.deltaTime;
            float normalized = 1f - (elapsed / patienceTime);

            // Color: green → yellow → red
            Color barColor;
            if (normalized > 0.5f)
                barColor = Color.Lerp(Color.yellow, Color.green, (normalized - 0.5f) * 2f);
            else
                barColor = Color.Lerp(Color.red, Color.yellow, normalized * 2f);

            bubble?.UpdatePatience(normalized, barColor);
            yield return null;
        }

        if (!orderFulfilled)
        {
            OnPatienceExpired();
        }
    }

    public void OnOrderFulfilled(float payment)
    {
        if (orderFulfilled) return;
        orderFulfilled = true;

        Debug.Log($"[Customer] Order fulfilled! Payment: {payment:F0} TL");

        if (patienceCoroutine != null) StopCoroutine(patienceCoroutine);
        if (animator != null) animator.SetTrigger(AnimHappy);
        if (spriteRenderer != null && happySprite != null) spriteRenderer.sprite = happySprite;

        PlayerData.Instance?.AddMoney(payment);
        AudioManager.Instance?.Play("coin_collect");
        StartCoroutine(AnimationHelper.PunchScale(transform, 0.2f, 0.3f));

        bubble?.Hide();
        StartCoroutine(LeaveAfterDelay(0.8f));
    }

    private void OnPatienceExpired()
    {
        if (orderFulfilled) return;
        orderFulfilled = true;

        if (animator != null) animator.SetTrigger(AnimAngry);
        if (spriteRenderer != null && angrySprite != null) spriteRenderer.sprite = angrySprite;
        AudioManager.Instance?.Play("customer_angry");

        bubble?.Hide();
        StartCoroutine(Leave());
    }

    /// <summary>
    /// Smoothly slide to a new queue position (called when queue shifts).
    /// </summary>
    public void SlideToPosition(Vector3 newTarget)
    {
        if (slideCoroutine != null) StopCoroutine(slideCoroutine);
        slideCoroutine = StartCoroutine(SlideRoutine(newTarget));
    }

    private IEnumerator SlideRoutine(Vector3 target)
    {
        if (animator != null) animator.SetBool(AnimWalk, true);

        while (Vector3.Distance(transform.position, target) > 0.05f)
        {
            if (!gameObject.activeSelf) yield break;
            transform.position = Vector3.MoveTowards(
                transform.position, target, moveSpeed * Time.deltaTime);
            yield return null;
        }

        transform.position = target;
        if (animator != null)
        {
            animator.SetBool(AnimWalk, false);
            animator.SetTrigger(AnimIdle);
        }

        slideCoroutine = null;
    }

    private IEnumerator Leave()
    {
        if (animator != null) animator.SetBool(AnimWalk, true);

        Vector3 exitTarget = transform.position + Vector3.right * 10f;

        while (Vector3.Distance(transform.position, exitTarget) > 0.1f)
        {
            if (!gameObject.activeSelf) yield break;
            transform.position = Vector3.MoveTowards(
                transform.position, exitTarget, moveSpeed * Time.deltaTime);
            yield return null;
        }

        CustomerManager.Instance?.CustomerLeft(this);
        Destroy(gameObject);
    }

    private IEnumerator LeaveAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        yield return StartCoroutine(Leave());
    }
}
