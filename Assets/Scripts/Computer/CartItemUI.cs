using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CartItemUI : MonoBehaviour
{
    [SerializeField] private Image productImage;
    [SerializeField] private TextMeshProUGUI nameAndQuantityText;
    [SerializeField] private TextMeshProUGUI priceText;
    [SerializeField] private Button removeButton;

    private ShoppingCart.CartItem currentItem;

    public void Setup(ShoppingCart.CartItem item)
    {
        currentItem = item;

        productImage.sprite = item.Product.productImage;
        nameAndQuantityText.text = $"x{item.Quantity} {item.Product.productName}";
        priceText.text = $"{(item.Product.price * item.Quantity)} $";

        if (removeButton != null) removeButton.onClick.AddListener(RemoveItemFromCart);
    }

    private void RemoveItemFromCart()
    {
        // DÜZELTÝLDÝ: Yorum satýrý kaldýrýldý ve artýk çalýþýyor.
        ShoppingCart.Instance.RemoveItem(currentItem);
    }
}