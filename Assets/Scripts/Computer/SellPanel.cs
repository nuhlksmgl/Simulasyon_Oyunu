// SellPanel.cs
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SellPanel : MonoBehaviour
{
    public InGameMarket inGameMarket;
    public Button[] sellButtons;
    public TMP_InputField[] inputFields;
    public TextMeshProUGUI[] marketPriceTexts; // <-- YENÝ EKLENEN DÝZÝ (Inspector'dan atanacak)

    public Dictionary<string, int> sellPrices = new Dictionary<string, int>();
    public Dictionary<string, int> sellQuantities = new Dictionary<string, int>();

    void Start()
    {
        // Start'ta veya OnEnable'da çaðýrmak daha iyi olabilir,
        // özellikle InGameMarket'in Start'ýndan sonra çalýþmasý için.
        // Þimdilik Start'ta býrakalým.
        if (inGameMarket != null && inGameMarket.products != null && inGameMarket.products.Length > 0)
        {
            UpdateSellPanel();
        }
        else
        {
            Debug.LogWarning("SellPanel: InGameMarket veya ürünleri henüz hazýr deðil. UpdateSellPanel çaðrýlmadý.");
        }
    }

    public void UpdateSellPanel()
    {
        if (inGameMarket == null || inGameMarket.products == null)
        {
            Debug.LogError("SellPanel: InGameMarket veya ürünleri atanmamýþ/yüklenmemiþ!");
            return;
        }

        // Dizilerin boyutlarýný kontrol et, en küçüðüne göre döngü kur veya hata ver
        int loopCount = Mathf.Min(sellButtons.Length, inputFields.Length, marketPriceTexts.Length, inGameMarket.products.Length);
        if (sellButtons.Length != inputFields.Length || sellButtons.Length != marketPriceTexts.Length)
        {
            Debug.LogWarning("SellPanel: Buton, InputField ve MarketPriceText dizilerinin boyutlarý eþleþmiyor! UI elemanlarýný kontrol edin.");
            // En küçük dizi boyutuna göre iþlem yapmaya devam edebilir veya burada durabilir.
            // Þimdilik en küçük olana göre devam edelim ama idealde eþit olmalýlar.
            loopCount = Mathf.Min(sellButtons.Length, inputFields.Length, marketPriceTexts.Length);
        }


        for (int i = 0; i < loopCount; i++) // loopCount kullanarak sýnýrlarý aþma
        {
            // if (i < inGameMarket.products.Length) // Bu kontrol artýk loopCount ile yapýlýyor
            // {
            InGameMarket.MarketProduct product = inGameMarket.products[i];
            TextMeshProUGUI buttonText = sellButtons[i].GetComponentInChildren<TextMeshProUGUI>();

            if (buttonText != null)
                buttonText.text = $"{product.productName} (Stok: {product.quantity})"; // Buton metnini güncelle

            // --- YENÝ EKLENEN KISIM ---
            if (marketPriceTexts[i] != null)
            {
                // currentAverageMarketPrice float olduðu için formatlayarak yazdýrabiliriz
                marketPriceTexts[i].text = $"Piyasa: {product.currentAverageMarketPrice:F0}$"; // F0 = ondalýksýz
            }
            // --- YENÝ EKLENEN KISIM SONU ---

            if (product.quantity > 0)
            {
                sellButtons[i].interactable = true;
                inputFields[i].interactable = true;
                int index = i;
                sellButtons[i].onClick.RemoveAllListeners();
                sellButtons[i].onClick.AddListener(() => SellProduct(inGameMarket.products[index], inputFields[index])); // products[index] ile doðru ürünü al
            }
            else
            {
                sellButtons[i].interactable = false;
                inputFields[i].interactable = false;
            }
            sellButtons[i].gameObject.SetActive(true); // Her zaman görünür yap, interactable ile kontrol et
            inputFields[i].gameObject.SetActive(true);
            if (marketPriceTexts[i] != null) marketPriceTexts[i].gameObject.SetActive(true);
            // }
            // else // Bu else bloðu artýk loopCount ile gereksiz
            // {
            //     sellButtons[i].gameObject.SetActive(false);
            //     inputFields[i].gameObject.SetActive(false);
            //     if (marketPriceTexts[i] != null) marketPriceTexts[i].gameObject.SetActive(false);
            // }
        }

        // Eðer UI elemanlarý ürün sayýsýndan fazlaysa, kalanlarý gizle
        for (int i = loopCount; i < sellButtons.Length; i++) sellButtons[i].gameObject.SetActive(false);
        for (int i = loopCount; i < inputFields.Length; i++) inputFields[i].gameObject.SetActive(false);
        for (int i = loopCount; i < marketPriceTexts.Length; i++) marketPriceTexts[i].gameObject.SetActive(false);


        Debug.Log("SellPanel güncellendi.");
    }

    void SellProduct(InGameMarket.MarketProduct product, TMP_InputField inputField)
    {
        int quantityToSell = 1;
        int priceToSell;

        if (product == null)
        {
            Debug.LogError("SellProduct: Ürün (product) null!");
            return;
        }
        if (inputField == null)
        {
            Debug.LogError("SellProduct: Fiyat giriþ alaný (inputField) null!");
            return;
        }


        if (int.TryParse(inputField.text, out priceToSell) && product.quantity >= quantityToSell) // Stok kontrolü >= quantityToSell olmalý
        {
            product.quantity -= quantityToSell; // Önce InGameMarket'teki stoðu azalt (bu oyuncunun satýlabilir stoðu)

            // sellQuantities ve sellPrices, ComputerOrderSystem'ýn otomatik satýþlarý için kullanýlýyordu.
            // Eðer CustomerOrderManager ile manuel sipariþlere geçtiysek, bu dictionary'lerin rolü deðiþebilir
            // veya sadece oyuncunun "listelediði fiyatý" kaydetmek için kullanýlabilir.
            // Þimdilik mevcut mantýðý koruyalým:
            if (sellQuantities.ContainsKey(product.productName))
            {
                sellQuantities[product.productName] += quantityToSell;
            }
            else
            {
                sellQuantities[product.productName] = quantityToSell;
            }
            sellPrices[product.productName] = priceToSell; // Oyuncunun bu ürün için belirlediði satýþ fiyatýný kaydet

            Debug.Log($"{product.productName} için satýþ fiyatý {priceToSell}$ olarak ayarlandý/güncellendi. Kalan Stok: {product.quantity}");
            UpdateSellPanel();
        }
        else
        {
            if (product.quantity < quantityToSell)
            {
                Debug.LogWarning($"{product.productName} için yetersiz stok! Satýþ yapýlamadý.");
            }
            else
            {
                Debug.LogWarning($"{product.productName} için geçersiz fiyat girildi: {inputField.text}");
            }
        }
    }

    // Bu Get metodlarý ComputerOrderSystem tarafýndan kullanýlýyordu.
    // Yeni CustomerOrderManager sistemi için rolleri tekrar deðerlendirilebilir.
    // CustomerOrderManager, fiyatlarý doðrudan sellPrices'dan okuyabilir.
    public Dictionary<string, int> GetSellQuantities()
    {
        return sellQuantities;
    }

    public Dictionary<string, int> GetSellPrices()
    {
        return sellPrices;
    }

    public void ClearSellQuantities()
    {
        sellQuantities.Clear();
    }
}