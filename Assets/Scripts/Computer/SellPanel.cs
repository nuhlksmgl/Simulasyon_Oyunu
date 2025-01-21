using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SellPanel : MonoBehaviour
{
    public InGameMarket inGameMarket; // InGameMarket script referans�
    public Button[] sellButtons; // Mevcut butonlar
    public TMP_InputField[] inputFields; // Mevcut TMP input field'lar
    public Dictionary<string, int> sellPrices = new Dictionary<string, int>(); // �r�nlerin sat�� fiyatlar�
    public Dictionary<string, int> sellQuantities = new Dictionary<string, int>(); // �r�nlerin sat�� miktarlar�

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
                    int index = i; // Lambda closure hatas�n� �nlemek i�in index kopyas�
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
        int quantityToSell = 1; // �rnek olarak her seferinde 1 adet sat�laca��n� varsay�yoruz
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
            sellPrices[product.productName] = priceToSell; // �r�n fiyat�n� kaydediyoruz
            Debug.Log($"{product.productName} sat��ta! Sat�� fiyat�: {priceToSell} $");
            UpdateSellPanel(); // Sat�� sonras� paneli g�ncelle
        }
        else
        {
            Debug.LogError("Yetersiz miktar veya ge�ersiz fiyat!");
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
