using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Customer : MonoBehaviour
{
    [SerializeField] private CustomerType customerType;
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private Animator animator;
    [SerializeField] private GameObject orderBubblePrefab;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite happySprite;
    [SerializeField] private Sprite angrySprite;
    [SerializeField] private Sprite vipSprite;

    private float patienceTime;
    private float bonusMultiplier;
    private Transform counterPosition;
    private float counterOffsetX;
    private CustomerOrderBubble bubble;
    private Coroutine patienceCoroutine;
    private Coroutine slideCoroutine;
    private bool orderFulfilled;
    private Order currentOrder;
    private CustomerTypeStats stats;

    private static readonly int AnimWalk = Animator.StringToHash("Walk");
    private static readonly string AnimIdle = "Idle";
    private static readonly string AnimHappy = "Happy";
    private static readonly string AnimAngry = "Angry";

    public CustomerType Type => customerType;
    public float BonusMultiplier => bonusMultiplier;

    public void Initialize(CustomerType type, Transform counter, float offsetX = 0f)
    {
        customerType = type;
        counterPosition = counter;
        counterOffsetX = offsetX;
        stats = CustomerData.Get(type);
        patienceTime = stats.patienceSeconds;
        bonusMultiplier = stats.paymentMultiplier;

        if (spriteRenderer != null)
        {
            spriteRenderer.color = stats.tintColor;
            if (type == CustomerType.VIP && vipSprite != null)
                spriteRenderer.sprite = vipSprite;
        }

        StartCoroutine(MoveToCounter());
    }

    private IEnumerator MoveToCounter()
    {
        if (animator != null) animator.SetBool(AnimWalk, true);
        Vector3 basePos = counterPosition != null ? counterPosition.position : transform.position + Vector3.left * 3f;
        Vector3 target = basePos + new Vector3(counterOffsetX, 0f, 0f);

        while (Vector3.Distance(transform.position, target) > 0.05f)
        {
            if (!gameObject.activeSelf) yield break;
            transform.position = Vector3.MoveTowards(transform.position, target, moveSpeed * Time.deltaTime);
            yield return null;
        }

        transform.position = target;
        if (animator != null)
        {
            animator.SetBool(AnimWalk, false);
            animator.SetTrigger(AnimIdle);
        }
        ShowOrder();
    }

    private void ShowOrder()
    {
        List<ProductData> products = GetUnlockedProducts();
        if (products.Count == 0) return;

        int lineCount = Random.Range(1, 4);
        Order order = new Order
        {
            customer = this,
            bonusMultiplier = bonusMultiplier
        };

        for (int i = 0; i < lineCount; i++)
        {
            ProductData product = products[Random.Range(0, products.Count)];
            int quantity = Random.Range(1, 3);
            order.lines.Add(new OrderLine { product = product, quantity = quantity, isPrepared = false });
        }

        order.payment = OrderManager.Instance != null ? OrderManager.Instance.CalculatePayment(order) : 0f;
        currentOrder = order;
        OrderManager.Instance?.RegisterOrder(order);

        if (orderBubblePrefab != null)
        {
            GameObject bubbleGO = Instantiate(orderBubblePrefab);
            bubble = bubbleGO.GetComponent<CustomerOrderBubble>();
            bubble?.AttachTo(transform, new Vector3(0f, 1.8f, 0f));
            bubble?.ShowOrder(order, stats);
        }

        patienceCoroutine = StartCoroutine(PatienceCountdown());
    }

    public void OnOrderLinePrepared()
    {
        if (currentOrder == null || bubble == null) return;
        bubble.ShowOrder(currentOrder, stats);
    }

    private List<ProductData> GetUnlockedProducts()
    {
        ProductData[] allProducts = CustomerManager.Instance != null ? CustomerManager.Instance.GetProductDataList() : null;
        List<ProductData> list = new List<ProductData>();
        if (allProducts == null) return list;
        int level = PlayerData.Instance != null ? PlayerData.Instance.shopLevel : 1;
        for (int i = 0; i < allProducts.Length; i++)
        {
            ProductData p = allProducts[i];
            if (p == null) continue;
            bool unlocked = ShopManager.Instance != null ? ShopManager.Instance.IsProductUnlocked(p) : p.unlockLevel <= level;
            if (unlocked) list.Add(p);
        }
        return list;
    }

    private IEnumerator PatienceCountdown()
    {
        float elapsed = 0f;
        while (elapsed < patienceTime)
        {
            if (!gameObject.activeSelf) yield break;
            elapsed += Time.deltaTime;
            float normalized = 1f - (elapsed / patienceTime);
            Color barColor = normalized > 0.5f
                ? Color.Lerp(Color.yellow, Color.green, (normalized - 0.5f) * 2f)
                : Color.Lerp(Color.red, Color.yellow, normalized * 2f);
            bubble?.UpdatePatience(normalized, barColor);
            yield return null;
        }

        if (!orderFulfilled) OnPatienceExpired();
    }

    public void OnOrderFulfilled(float payment)
    {
        if (orderFulfilled) return;
        orderFulfilled = true;

        if (patienceCoroutine != null) StopCoroutine(patienceCoroutine);
        if (animator != null) animator.SetTrigger(AnimHappy);
        if (spriteRenderer != null && happySprite != null) spriteRenderer.sprite = happySprite;

        PlayerData.Instance?.AddMoney(payment);
        AudioManager.Instance?.Play("coin_collect");
        GameEvents.RaiseCustomerServed(customerType);
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
            transform.position = Vector3.MoveTowards(transform.position, target, moveSpeed * Time.deltaTime);
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
            transform.position = Vector3.MoveTowards(transform.position, exitTarget, moveSpeed * Time.deltaTime);
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
