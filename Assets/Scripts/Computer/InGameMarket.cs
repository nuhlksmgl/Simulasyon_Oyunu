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
        // Buy butonlarýnýn click eventlerini baðlayalým
        Button[] buyButtons = GetComponentsInChildren<Button>();
        for (int i = 0; i < buyButtons.Length; i++)
        {
            int index = i; // Lambda closure hatasýný önlemek için index kopyasý
            buyButtons[i].onClick.AddListener(() => BuyProduct(index));
        }
    }

    void BuyProduct(int productIndex)
    {
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
