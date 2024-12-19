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
        // Buy butonlar�n�n click eventlerini ba�layal�m
        Button[] buyButtons = GetComponentsInChildren<Button>();
        for (int i = 0; i < buyButtons.Length; i++)
        {
            int index = i; // Lambda closure hatas�n� �nlemek i�in index kopyas�
            buyButtons[i].onClick.AddListener(() => BuyProduct(index));
        }
    }

    void BuyProduct(int productIndex)
    {
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
