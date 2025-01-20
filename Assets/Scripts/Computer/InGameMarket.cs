using UnityEngine;
using UnityEngine.UI;

public class InGameMarket : MonoBehaviour
{
    [System.Serializable]
    public class Product
    {
        public string productName; // Ürün adý
        public int price;          // Ürün fiyatý
        public GameObject productPrefab; // 3D model prefabý
    }

    public Product[] products; // Ürün listesi
    public Transform spawnPoint; // Satýn alýnan ürünü spawn edeceðimiz nokta
    public PlayerBalance playerBalance; // PlayerBalance script'i referansý

    void Start()
    {
        // Tüm butonlarý al
        Button[] buttons = GetComponentsInChildren<Button>();
        for (int i = 0; i < buttons.Length; i++)
        {
            int index = i; // Lambda closure hatasýný önlemek için index kopyasý

            // Butonun tag'ini kontrol et
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

        // Bakiyeyi kontrol et
        if (playerBalance.DeductBalance(product.price))
        {
            Debug.Log($"{product.productName} satýn alýndý!");
            // Ürünü sahneye ekle
            Instantiate(product.productPrefab, spawnPoint.position, Quaternion.identity);
        }
        else
        {
            Debug.Log("Yetersiz bakiye!");
        }
    }
}
