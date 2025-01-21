using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SellPanel : MonoBehaviour
{
    public InGameMarket inGameMarket; // InGameMarket script referansý
    public Button[] sellButtons; // Mevcut butonlar
    public TMP_InputField[] inputFields; // Mevcut TMP input field'lar
    public Dictionary<string, int> sellPrices = new Dictionary<string, int>(); // Ürünlerin satýþ fiyatlarý
    public Dictionary<string, int> sellQuantities = new Dictionary<string, int>(); // Ürünlerin satýþ miktarlarý

    void Start()
    {
        UpdateSellPanel();
    }

    public void UpdateSellPanel()
    {
        for (int i = 0; i < sellButtons.Length; i++)
        {
            if (i < inGameMarket.products.Length)
            {
                InGameMarket.Product product = inGameMarket.products[i];
                TextMeshProUGUI buttonText = sellButtons[i].GetComponentInChildren<TextMeshProUGUI>();
                buttonText.text = $"{product.productName} ({product.quantity})";

                if (product.quantity > 0)
                {
                    sellButtons[i].interactable = true;
                    inputFields[i].interactable = true;
                    int index = i; // Lambda closure hatasýný önlemek için index kopyasý
                    sellButtons[i].onClick.RemoveAllListeners();
                    sellButtons[i].onClick.AddListener(() => SellProduct(product, inputFields[index]));
                }
                else
                {
                    sellButtons[i].interactable = false;
                    inputFields[i].interactable = false;
                }
            }
            else
            {
                sellButtons[i].gameObject.SetActive(false);
                inputFields[i].gameObject.SetActive(false);
            }
        }
    }

    void SellProduct(InGameMarket.Product product, TMP_InputField inputField)
    {
        int quantityToSell = 1; // Örnek olarak her seferinde 1 adet satýlacaðýný varsayýyoruz
        int priceToSell;

        if (int.TryParse(inputField.text, out priceToSell) && product.quantity > 0)
        {
            product.quantity -= quantityToSell;
            if (sellQuantities.ContainsKey(product.productName))
            {
                sellQuantities[product.productName] += quantityToSell;
            }
            else
            {
                sellQuantities[product.productName] = quantityToSell;
            }
            sellPrices[product.productName] = priceToSell; // Ürün fiyatýný kaydediyoruz
            Debug.Log($"{product.productName} satýþta! Satýþ fiyatý: {priceToSell} $");
            UpdateSellPanel(); // Satýþ sonrasý paneli güncelle
        }
        else
        {
            Debug.LogError("Yetersiz miktar veya geçersiz fiyat!");
        }
    }

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
