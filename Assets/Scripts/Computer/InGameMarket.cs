using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class InGameMarket : MonoBehaviour
{
    [System.Serializable]
    public class Product
    {
        public string productName; // Ürün adý
        public int price;          // Ürün fiyatý
        public GameObject productPrefab; // 3D model prefabý
        public int quantity = 0;   // Ürün miktarý
    }

    public Product[] products; // Ürün listesi
    public Transform spawnPoint; // Satýn alýnan ürünü spawn edeceðimiz nokta
    public PlayerBalance playerBalance; // PlayerBalance script'i referansý
    public SellPanel sellPanel; // SellPanel script referansý

    void Start()
    {
        Button[] buttons = GetComponentsInChildren<Button>();
        for (int i = 0; i < buttons.Length; i++)
        {
            int index = i; // Lambda closure hatasýný önlemek için index kopyasý

            if (buttons[i].CompareTag("BuyButton")) // Sadece BuyButton tag'ine sahip butonlar için
            {
                buttons[i].onClick.AddListener(() => BuyProduct(index));
            }
        }
    }

    void BuyProduct(int productIndex)
    {
        if (productIndex < 0 || productIndex >= products.Length)
        {
            Debug.LogError("Geçersiz ürün indeksi!");
            return;
        }

        Product product = products[productIndex];

        if (playerBalance.DeductBalance(product.price))
        {
            Debug.Log($"{product.productName} satýn alýndý!");
            Instantiate(product.productPrefab, spawnPoint.position, Quaternion.identity);
            product.quantity++;
            sellPanel.UpdateSellPanel(); // Satýn alma sonrasý satýþ panelini güncelle
        }
        else
        {
            Debug.Log("Yetersiz bakiye!");
        }
    }

    public List<Product> GetAvailableProducts()
    {
        List<Product> availableProducts = new List<Product>();
        foreach (Product product in products)
        {
            if (product.quantity > 0)
            {
                availableProducts.Add(product);
            }
        }
        return availableProducts;
    }
}
