using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class OrderItemIconUI : MonoBehaviour
{
    [SerializeField] private Image productImage;
    [SerializeField] private TextMeshProUGUI quantityText;

    // Bu metot, OrderListPanelUI tarafýndan çaðrýlarak ikonun bilgilerini doldurur
    public void Setup(OrderItemDetail item)
    {
        if (item == null || item.productDefinition == null) return;

        // Ürünün resmini ata
        if (productImage != null && item.productDefinition.productImage != null)
        {
            productImage.sprite = item.productDefinition.productImage;
        }

        // Miktar metnini ata
        if (quantityText != null)
        {
            quantityText.text = $"x{item.quantity}";
        }
    }
}