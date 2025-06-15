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
            // Input alanýna bir fiyat girilip enter'a basýldýðýnda veya dýþarý týklandýðýnda
            // OnPriceChanged metodunu çaðýracak bir listener ekliyoruz.
            priceInputField.onEndEdit.AddListener(OnPriceChanged);
        }
    }

    /// <summary>
    /// Kartýn bilgilerini doldurur. SellPanelManager tarafýndan çaðrýlýr.
    /// </summary>
    public void Setup(MarketProduct product)
    {
        this.currentProduct = product;

        // Kartýn görselini ve yazýlarýný ürün verileriyle doldur
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
            // Fiziksel stoðu gösterir
            stokText.text = $"Stok: {product.physicalStock}";
        }

        if (marketPriceText != null)
        {
            marketPriceText.text = $"Piyasa: {product.currentAverageMarketPrice:F0}$";
        }

        if (priceInputField != null)
        {
            // Input alanýna ürünün mevcut satýþ fiyatýný yaz
            priceInputField.text = product.price.ToString();
        }
    }

    /// <summary>
    /// Oyuncu yeni bir fiyat girdiðinde bu metot çaðrýlýr.
    /// </summary>
    private void OnPriceChanged(string newPriceString)
    {
        if (currentProduct == null) return;

        // Input alanýna girilen metni sayýya çevirmeye çalýþ
        if (int.TryParse(newPriceString, out int newPrice))
        {
            // Baþarýlý olursa, ürünün satýþ fiyatýný güncelle
            currentProduct.price = newPrice;
            Debug.Log($"{currentProduct.productName} ürününün yeni satýþ fiyatý {newPrice}$ olarak ayarlandý.");
        }
        else
        {
            // Geçersiz bir deðer girilirse, input alanýný ürünün son geçerli fiyatýyla doldur.
            priceInputField.text = currentProduct.price.ToString();
            Debug.LogWarning("Geçersiz fiyat girildi. Deðiþiklik yapýlmadý.");
        }
    }
}