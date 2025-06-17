using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ProductCardUI : MonoBehaviour
{
    [Header("UI Referansları")]
    [SerializeField] private Image productImage;
    [SerializeField] private TextMeshProUGUI productNameText;
    [SerializeField] private TextMeshProUGUI priceText;

    [Header("Miktar Kontrolü")]
    [SerializeField] private Button plusButton;
    [SerializeField] private Button minusButton;
    [SerializeField] private TextMeshProUGUI quantityText;
    [SerializeField] private Button addToCartButton;

    private MarketProduct currentProduct;
    private Category parentCategory;
    private InGameMarket marketController;
    private int currentQuantity = 1;

    void Awake()
    {
        plusButton.onClick.AddListener(IncreaseQuantity);
        minusButton.onClick.AddListener(DecreaseQuantity);
        addToCartButton.onClick.AddListener(OnAddToCartClicked);
    }

    public void Setup(MarketProduct product, Category category, InGameMarket market)
    {
        this.currentProduct = product;
        this.parentCategory = category;
        this.marketController = market;

        if (productImage != null && product.productImage != null)
        {
            productImage.sprite = product.productImage;
        }
        if (productNameText != null)
        {
            productNameText.text = product.productName;
        }
        if (priceText != null)
        {
            priceText.text = $"{product.price}₺";
        }

        currentQuantity = 1;
        UpdateQuantityText();

        // Butonların durumunu ayarla
        addToCartButton.interactable = true;
        addToCartButton.GetComponentInChildren<TextMeshProUGUI>().text = "";

        if (product.isOneTimePurchase && product.isPurchased)
        {
            addToCartButton.interactable = false;
            addToCartButton.GetComponentInChildren<TextMeshProUGUI>().text = "Satın Alındı";
        }
        else if (product.productName == "Raf")
        {
            if (marketController != null && !marketController.CanPurchaseShelf())
            {
                addToCartButton.interactable = false;
                addToCartButton.GetComponentInChildren<TextMeshProUGUI>().text = "Limit Dolu";
            }
        }
    }

    private void IncreaseQuantity()
    {
        currentQuantity++;
        UpdateQuantityText();
    }

    private void DecreaseQuantity()
    {
        if (currentQuantity > 1)
        {
            currentQuantity--;
            UpdateQuantityText();
        }
    }

    private void UpdateQuantityText()
    {
        if (quantityText != null)
        {
            quantityText.text = currentQuantity.ToString();
        }
    }

    private void OnAddToCartClicked()
    {
        if (currentProduct != null && ShoppingCart.Instance != null)
        {
            ShoppingCart.Instance.AddItem(currentProduct, currentQuantity);
        }
    }
}