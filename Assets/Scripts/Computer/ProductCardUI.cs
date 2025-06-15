using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ProductCardUI : MonoBehaviour
{
    [Header("UI Referansları")]
    [SerializeField] private Image productImage;
    [SerializeField] private TextMeshProUGUI productNameText;
    [SerializeField] private TextMeshProUGUI priceText;

    // YENİ EKLENEN UI REFERANSLARI
    [Header("Miktar Kontrolü")]
    [SerializeField] private Button plusButton;
    [SerializeField] private Button minusButton;
    [SerializeField] private TextMeshProUGUI quantityText;
    [SerializeField] private Button addToCartButton; // Eski buyButton

    private MarketProduct currentProduct;
    private Category parentCategory;
    private InGameMarket marketController;

    // YENİ EKLENEN DEĞİŞKEN
    private int currentQuantity = 1;

    void Awake()
    {
        // Butonların listener'larını bir kereye mahsus burada atıyoruz.
        plusButton.onClick.AddListener(IncreaseQuantity);
        minusButton.onClick.AddListener(DecreaseQuantity);
        addToCartButton.onClick.AddListener(OnAddToCartClicked);
    }

    public void Setup(MarketProduct product, Category category, InGameMarket market)
    {
        this.currentProduct = product;
        this.parentCategory = category;
        this.marketController = market;

        // Kart bilgilerini doldur
        if (productImage != null) productImage.sprite = product.productImage;
        if (productNameText != null) productNameText.text = product.productName;
        if (priceText != null) priceText.text = $"{product.price}₺";

        // Miktarı sıfırla ve metni güncelle
        currentQuantity = 1;
        UpdateQuantityText();

        // Butonların durumunu ayarla (eski kodumuz)
        addToCartButton.interactable = true;
        addToCartButton.GetComponentInChildren<TextMeshProUGUI>().text = "Sepete Ekle";

        if (product.isOneTimePurchase && product.isPurchased)
        {
            addToCartButton.interactable = false;
            addToCartButton.GetComponentInChildren<TextMeshProUGUI>().text = "Satın Alındı";
        }
        else if (product.productName == "Raf" && !marketController.CanPurchaseShelf())
        {
            addToCartButton.interactable = false;
            addToCartButton.GetComponentInChildren<TextMeshProUGUI>().text = "Limit Dolu";
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
        if (currentProduct != null && marketController != null && parentCategory != null)
        {
            // Sepete eklerken artık miktarı da gönderiyoruz
            marketController.AddToPurchaseBasket(currentProduct, parentCategory, currentQuantity);
        }
    }
}