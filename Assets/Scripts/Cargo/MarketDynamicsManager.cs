using UnityEngine;
using System.Collections.Generic;

public class MarketDynamicsManager : MonoBehaviour
{
    public InGameMarket inGameMarket;
    public float priceChangeInterval = 300f;
    public float maxPriceFluctuation = 0.05f; // Ýsteðiniz üzerine %5 olarak ayarlandý

    private void Start()
    {
        if (inGameMarket == null) { /* Hata kontrolü */ return; }
        InvokeRepeating("UpdateMarketPrices", priceChangeInterval, priceChangeInterval);
    }

    void UpdateMarketPrices()
    {
        if (inGameMarket == null) return;

        foreach (Category category in inGameMarket.productCategories)
        {
            foreach (MarketProduct product in category.productsInCategory)
            {
                // Sadece bir temel fiyatý olan ürünlerin piyasa fiyatýný güncelle
                if (product.baseMarketPrice > 0)
                {
                    UpdateProductPrice(product);
                }
            }
        }
        Debug.Log("Piyasa fiyatlarý güncellendi.");
    }

    void UpdateProductPrice(MarketProduct product)
    {
        // Temel fiyat üzerinden rastgele bir dalgalanma uygula
        float randomFluctuation = Random.Range(1.0f - maxPriceFluctuation, 1.0f + maxPriceFluctuation);
        float newPrice = product.baseMarketPrice * randomFluctuation;

        product.currentAverageMarketPrice = Mathf.Max(1, Mathf.RoundToInt(newPrice));
    }
}