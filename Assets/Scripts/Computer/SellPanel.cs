// SellPanel.cs
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class SellPanel : MonoBehaviour
{
    public InGameMarket inGameMarket;
    public Button[] sellButtons;
    public TMP_InputField[] inputFields;
    public TextMeshProUGUI[] marketPriceTexts;

    // ComputerOrderSystem'in ihtiyaç duyduðu Dictionaries
    public Dictionary<string, int> sellPrices = new Dictionary<string, int>();
    public Dictionary<string, int> sellQuantities = new Dictionary<string, int>();

    void OnEnable()
    {
        if (inGameMarket != null)
        {
            UpdateSellPanel();
        }
    }

    public void UpdateSellPanel()
    {
        if (inGameMarket == null) return;

        // InGameMarket'tan tüm kilidi açýk ürünleri al
        List<MarketProduct> allProducts = inGameMarket.GetAllUnlockedProducts();

        int loopCount = Mathf.Min(sellButtons.Length, inputFields.Length, marketPriceTexts.Length, allProducts.Count);

        for (int i = 0; i < loopCount; i++)
        {
            MarketProduct product = allProducts[i];
            sellButtons[i].GetComponentInChildren<TextMeshProUGUI>().text = $"{product.productName} (Stok: {product.quantity})";
            marketPriceTexts[i].text = $"Piyasa: {product.currentAverageMarketPrice:F0}$";

            sellButtons[i].interactable = true;
            inputFields[i].interactable = true;

            int index = i;
            sellButtons[i].onClick.RemoveAllListeners();
            sellButtons[i].onClick.AddListener(() => SetProductForSale(allProducts[index], inputFields[index]));

            sellButtons[i].gameObject.SetActive(true);
            inputFields[i].gameObject.SetActive(true);
            marketPriceTexts[i].gameObject.SetActive(true);
        }

        // Kullanýlmayan UI elemanlarýný gizle
        for (int i = loopCount; i < sellButtons.Length; i++) sellButtons[i].gameObject.SetActive(false);
        for (int i = loopCount; i < inputFields.Length; i++) inputFields[i].gameObject.SetActive(false);
        for (int i = loopCount; i < marketPriceTexts.Length; i++) marketPriceTexts[i].gameObject.SetActive(false);
    }

    void SetProductForSale(MarketProduct product, TMP_InputField inputField)
    {
        if (product == null || inputField == null) return;

        if (int.TryParse(inputField.text, out int priceToSell))
        {
            product.price = priceToSell;

            // ComputerOrderSystem için satýþ listesini güncelle
            if (sellQuantities.ContainsKey(product.productName))
            {
                sellQuantities[product.productName]++;
            }
            else
            {
                sellQuantities[product.productName] = 1;
            }
            sellPrices[product.productName] = priceToSell;

            Debug.Log($"{product.productName} için satýþ fiyatý {priceToSell}$ olarak ayarlandý.");
        }
    }

    // --- ComputerOrderSystem ÝÇÝN EKLENEN METOTLAR ---
    public Dictionary<string, int> GetSellQuantities()
    {
        return sellQuantities;
    }

    public Dictionary<string, int> GetSellPrices()
    {
        return sellPrices;
    }
}