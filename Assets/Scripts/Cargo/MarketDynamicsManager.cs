using UnityEngine;
using System.Collections.Generic;

public class MarketDynamicsManager : MonoBehaviour
{
    public InGameMarket inGameMarket;
    public float priceChangeInterval = 300f; // 5 dakika
    public float maxPriceFluctuation = 0.2f; // Maksimum %20 dalgalanma
    public int trendChance = 30; // %30 ihtimalle trend baþlar/devam eder
    public int maxTrendStreak = 3; // Bir ürün en fazla 3 periyot art arda trendde kalabilir

    private void Start()
    {
        if (inGameMarket == null)
        {
            Debug.LogError("InGameMarket referansý atanmamýþ!");
            return;
        }
        InvokeRepeating("UpdateMarketPrices", priceChangeInterval, priceChangeInterval);
    }

    void UpdateMarketPrices()
    {
        if (inGameMarket == null) return;

        // DÜZELTME: Döngü artýk yeni kategori yapýsýna göre çalýþýyor.
        foreach (Category category in inGameMarket.productCategories)
        {
            foreach (MarketProduct product in category.productsInCategory)
            {
                UpdateProductPrice(product);
            }
        }

        Debug.Log("Piyasa fiyatlarý güncellendi.");

        // UI'ýn yenilenmesi için InGameMarket'a bir sinyal gönderebiliriz.
        // Örneðin: inGameMarket.RefreshUI(); (InGameMarket'a böyle bir metot eklenirse)
    }

    void UpdateProductPrice(MarketProduct product)
    {
        // Bu fonksiyonun iç mantýðý sizin tasarýmýnýza göre doðru olduðundan deðiþtirilmedi.
        // Sadece artýk doðru veri yapýsý üzerinden çaðrýlýyor.
        float basePrice = product.baseMarketPrice;
        int trendDirection = 0; // 0: no trend, 1: up, -1: down

        if (Random.Range(0, 100) < trendChance)
        {
            if (product.priceTrendStreak > 0)
                trendDirection = 1;
            else if (product.priceTrendStreak < 0)
                trendDirection = -1;
            else
                trendDirection = (Random.value > 0.5f) ? 1 : -1;
        }

        if (trendDirection != 0)
        {
            if (Mathf.Sign(product.priceTrendStreak) == trendDirection)
            {
                if (Mathf.Abs(product.priceTrendStreak) < maxTrendStreak)
                    product.priceTrendStreak += trendDirection;
                else
                    product.priceTrendStreak = 0;
            }
            else
            {
                product.priceTrendStreak = trendDirection;
            }
        }
        else
        {
            product.priceTrendStreak = 0;
        }

        float trendMultiplier = 1.0f + (product.priceTrendStreak * 0.1f);
        float randomFluctuation = Random.Range(-maxPriceFluctuation, maxPriceFluctuation);
        float newPrice = basePrice * trendMultiplier * (1.0f + randomFluctuation);

        product.currentAverageMarketPrice = Mathf.Max(1, Mathf.RoundToInt(newPrice));
    }
}