using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class InGameMarket : MonoBehaviour
{
    [System.Serializable]
    public class Product
    {
        public string productName; // �r�n ad�
        public int price;          // �r�n fiyat�
        public GameObject productPrefab; // 3D model prefab�
        public int quantity = 0;   // �r�n miktar�
    }

    public Product[] products; // �r�n listesi
    public Transform spawnPoint; // Sat�n al�nan �r�n� spawn edece�imiz nokta
    public PlayerBalance playerBalance; // PlayerBalance script'i referans�
    public SellPanel sellPanel; // SellPanel script referans�

    void Start()
    {
        Button[] buttons = GetComponentsInChildren<Button>();
        for (int i = 0; i < buttons.Length; i++)
        {
            int index = i; // Lambda closure hatas�n� �nlemek i�in index kopyas�

            if (buttons[i].CompareTag("BuyButton")) // Sadece BuyButton tag'ine sahip butonlar i�in
            {
                buttons[i].onClick.AddListener(() => BuyProduct(index));
            }
        }
    }

    void BuyProduct(int productIndex)
    {
        if (productIndex < 0 || productIndex >= products.Length)
        {
            Debug.LogError("Ge�ersiz �r�n indeksi!");
            return;
        }

        Product product = products[productIndex];

        if (playerBalance.DeductBalance(product.price))
        {
            Debug.Log($"{product.productName} sat�n al�nd�!");
            Instantiate(product.productPrefab, spawnPoint.position, Quaternion.identity);
            product.quantity++;
            sellPanel.UpdateSellPanel(); // Sat�n alma sonras� sat�� panelini g�ncelle
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
