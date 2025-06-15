using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class SellPanelManager : MonoBehaviour
{
    [Header("Referanslar")]
    [SerializeField] private InGameMarket inGameMarket;
    [SerializeField] private Transform contentParent;
    [SerializeField] private GameObject productSellCardPrefab;

    // Panel her açýldýðýnda veya aktif olduðunda listeyi yenile
    void OnEnable()
    {
        RefreshPanel();
    }

    public void RefreshPanel()
    {
        if (contentParent != null)
        {
            foreach (Transform child in contentParent)
            {
                Destroy(child.gameObject);
            }
        }

        if (inGameMarket == null || productSellCardPrefab == null)
        {
            Debug.LogError("SellPanelManager'da referanslar eksik!");
            return;
        }

        List<MarketProduct> productsInStock = new List<MarketProduct>();
        foreach (var category in inGameMarket.productCategories)
        {
            if (category.productsInCategory != null)
            {
                // DÜZELTME: Artýk 'quantity' yerine 'physicalStock' kontrolü yapýlýyor.
                productsInStock.AddRange(category.productsInCategory.Where(p => p.physicalStock > 0));
            }
        }

        foreach (var product in productsInStock)
        {
            GameObject cardInstance = Instantiate(productSellCardPrefab, contentParent);
            cardInstance.GetComponent<ProductSellCardUI>().Setup(product);
        }
    }
}