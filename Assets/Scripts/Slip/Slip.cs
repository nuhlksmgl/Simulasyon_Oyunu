using UnityEngine;
using TMPro;
using System.Text;

public class Slip : MonoBehaviour
{
    private OrderData orderData;
    [SerializeField] private Renderer slipRenderer;
    [SerializeField] private TextMeshProUGUI textMeshPro;
    [SerializeField] private Color highlightColor = Color.yellow;

    private Color originalColor;
    private bool isHeld;
    private Vector3 originalScale;

    void Awake()
    {
        // Oyun başladığında objenin orijinal ölçeğini kaydet
        originalScale = transform.localScale;

        if (slipRenderer == null) slipRenderer = GetComponent<Renderer>();
        if (slipRenderer != null)
        {
            Color initialColor = slipRenderer.material.color;
            if (initialColor.a < 1.0f)
            {
                initialColor.a = 1.0f;
                slipRenderer.material.color = initialColor;
            }
            originalColor = initialColor;
        }
        else
        {
            Debug.LogError($"[Slip.Awake] {name} üzerinde Renderer komponenti eksik!");
        }

        if (textMeshPro == null) textMeshPro = GetComponentInChildren<TextMeshProUGUI>();
        if (textMeshPro != null)
        {
            textMeshPro.gameObject.SetActive(false);
        }
    }

    public Vector3 GetOriginalScale()
    {
        return originalScale;
    }

    public void SetOrderData(OrderData order)
    {
        orderData = order;
        UpdateText();
    }

    public OrderData GetOrderData()
    {
        return orderData;
    }

    public void Highlight(bool highlight)
    {
        if (slipRenderer == null) return;
        slipRenderer.material.color = highlight ? highlightColor : originalColor;
    }

    public void OnPickedUp()
    {
        isHeld = true;
        if (textMeshPro != null) textMeshPro.gameObject.SetActive(true);
    }

    public void OnDropped()
    {
        isHeld = false;
    }

    private void UpdateText()
    {
        if (orderData == null || textMeshPro == null) return;

        StringBuilder builder = new StringBuilder();
        builder.AppendLine($"Sipariş ID: {orderData.orderID}");
        builder.AppendLine($"Müşteri: {orderData.customerName}");
        builder.AppendLine("<b>Ürünler:</b>");

        if (orderData.itemsInOrder != null && orderData.itemsInOrder.Count > 0)
        {
            foreach (var item in orderData.itemsInOrder)
            {
                string productName = (item.productDefinition != null) ? item.productDefinition.productName : "[Tanımsız]";
                builder.AppendLine($"- {productName} x{item.quantity}");
            }
        }
        else
        {
            builder.AppendLine("(Sipariş Kalemi Yok)");
        }
        builder.AppendLine($"<b>Toplam: {orderData.totalOrderValue:C2}</b>");

        textMeshPro.text = builder.ToString();
    }
}