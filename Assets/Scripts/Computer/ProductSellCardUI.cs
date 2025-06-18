using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ProductSellCardUI : MonoBehaviour
{
    [Header("UI Referanslarý")]
    [SerializeField] private Image productImage;
    [SerializeField] private TextMeshProUGUI productNameText;
    [SerializeField] private TextMeshProUGUI stokText;
    [SerializeField] private TextMeshProUGUI marketPriceText;
    [SerializeField] private TMP_InputField priceInputField;

    private MarketProduct currentProduct;

    void Awake()
    {
        if (priceInputField != null)
        {
            priceInputField.onEndEdit.AddListener(OnPriceChanged);
        }
    }

    public void Setup(MarketProduct product)
    {
        this.currentProduct = product;

        if (productImage != null && product.productImage != null)
        {
            productImage.sprite = product.productImage;
        }

        if (productNameText != null)
        {
            productNameText.text = product.productName;
        }

        if (stokText != null)
        {
            stokText.text = $"Stok: {product.physicalStock}";
        }

        if (marketPriceText != null)
        {
            marketPriceText.text = $"Piyasa: {product.currentAverageMarketPrice:F0}$";
        }

        if (priceInputField != null)
        {
            priceInputField.text = product.price.ToString();
        }
    }

    private void OnPriceChanged(string newPriceString)
    {
        if (currentProduct == null) return;

        if (int.TryParse(newPriceString, out int newPrice) && newPrice > 0)
        {
            currentProduct.price = newPrice;

            // ÖNEMLÝ: Ürünü satýþa koyuldu olarak iþaretle
            currentProduct.isListedForSale = true;

            Debug.Log($"{currentProduct.productName} ürününün yeni satýþ fiyatý {newPrice}$ olarak ayarlandý ve satýþa konuldu.");
        }
        else
        {
            priceInputField.text = currentProduct.price.ToString();
            Debug.LogWarning("Geçersiz fiyat girildi. Deðiþiklik yapýlmadý.");
        }
    }
}