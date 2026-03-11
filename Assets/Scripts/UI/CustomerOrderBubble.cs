using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CustomerOrderBubble : MonoBehaviour
{
    [SerializeField] private Image productIcon;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private TMP_Text productNameText;
    [SerializeField] private TMP_Text quantityText;
    [SerializeField] private Image patienceBar;
    [SerializeField] private TMP_Text vipBadgeText;
    [SerializeField] private Vector3 localFollowOffset = new Vector3(0f, 1.8f, 0f);

    public void AttachTo(Transform owner, Vector3 localOffset)
    {
        if (owner == null) return;
        transform.SetParent(owner, false);
        localFollowOffset = localOffset;
        transform.localPosition = localFollowOffset;
    }

    public void Show(ProductData product, int quantity)
    {
        gameObject.SetActive(true);

        if (productIcon != null && product != null && product.productIcon != null)
            productIcon.sprite = product.productIcon;

        if (productNameText != null)
            productNameText.text = product != null ? product.productName : "URUN";

        if (quantityText != null)
            quantityText.text = $"x{Mathf.Max(1, quantity)}";

        if (patienceBar != null)
        {
            patienceBar.fillAmount = 1f;
            patienceBar.color = Color.green;
        }
    }

    public void ShowOrder(Order order, CustomerTypeStats stats)
    {
        gameObject.SetActive(true);
        if (order == null) return;

        if (backgroundImage != null)
            backgroundImage.color = stats != null ? stats.bubbleTint : new Color(0.96f, 0.96f, 0.96f, 1f);

        if (productNameText != null)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            for (int i = 0; i < order.lines.Count; i++)
            {
                var line = order.lines[i];
                if (line.product == null) continue;
                sb.Append(line.product.productName);
                sb.Append(" x");
                sb.Append(Mathf.Max(1, line.quantity));
                if (i < order.lines.Count - 1) sb.Append('\n');
            }
            productNameText.text = sb.ToString();
        }

        if (quantityText != null)
            quantityText.text = string.Empty;

        if (vipBadgeText != null)
        {
            bool isVip = stats != null && stats.vipBadge;
            vipBadgeText.gameObject.SetActive(isVip);
            vipBadgeText.text = isVip ? "VIP" : string.Empty;
        }

        if (patienceBar != null)
        {
            patienceBar.fillAmount = 1f;
            patienceBar.color = Color.green;
        }
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    public void UpdatePatience(float normalizedValue, Color barColor)
    {
        if (patienceBar != null)
        {
            patienceBar.fillAmount = normalizedValue;
            patienceBar.color = barColor;
        }
    }

    void LateUpdate()
    {
        if (transform.parent != null)
            transform.localPosition = localFollowOffset;

        if (Camera.main == null) return;

        Vector3 camForward = Camera.main.transform.forward;
        transform.rotation = Quaternion.LookRotation(camForward, Vector3.up);
    }
}
