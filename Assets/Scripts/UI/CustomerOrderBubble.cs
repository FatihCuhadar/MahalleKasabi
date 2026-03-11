using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CustomerOrderBubble : MonoBehaviour
{
    [SerializeField] private Image productIcon;
    [SerializeField] private TMP_Text productNameText;
    [SerializeField] private TMP_Text quantityText;
    [SerializeField] private Image patienceBar;
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
