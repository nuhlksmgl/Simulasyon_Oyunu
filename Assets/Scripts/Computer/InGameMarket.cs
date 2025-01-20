using UnityEngine;
using UnityEngine.UI;

public class InGameMarket : MonoBehaviour
{
    [System.Serializable]
    public class Product
    {
        public string productName; // �r�n ad�
        public int price;          // �r�n fiyat�
        public GameObject productPrefab; // 3D model prefab�
    }

    public Product[] products; // �r�n listesi
    public Transform spawnPoint; // Sat�n al�nan �r�n� spawn edece�imiz nokta
    public PlayerBalance playerBalance; // PlayerBalance script'i referans�

    void Start()
    {
        // T�m butonlar� al
        Button[] buttons = GetComponentsInChildren<Button>();
        for (int i = 0; i < buttons.Length; i++)
        {
            int index = i; // Lambda closure hatas�n� �nlemek i�in index kopyas�

            // Butonun tag'ini kontrol et
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

        // Bakiyeyi kontrol et
        if (playerBalance.DeductBalance(product.price))
        {
            Debug.Log($"{product.productName} sat�n al�nd�!");
            // �r�n� sahneye ekle
            Instantiate(product.productPrefab, spawnPoint.position, Quaternion.identity);
        }
        else
        {
            Debug.Log("Yetersiz bakiye!");
        }
    }
}
