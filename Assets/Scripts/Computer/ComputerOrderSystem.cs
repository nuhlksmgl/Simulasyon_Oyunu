using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class ComputerOrderSystem : MonoBehaviour
{
    public InGameMarket inGameMarket; // InGameMarket script referansý
    public PlayerBalance playerBalance; // PlayerBalance script referansý
    public SellPanel sellPanel; // SellPanel script referansý
    public float orderInterval = 10.0f; // Sipariþ verme aralýðý
    private float nextOrderTime = 0.0f;
    public TextMeshProUGUI orderPanelText; // Sipariþleri gösterecek TextMeshPro UI elemaný
    public GameObject orderPanel; // Sipariþ paneli
    private float orderDisplayDuration = 60.0f; // Panelin açýk kalma süresi
    private float orderDisplayEndTime = 0.0f; // Panelin kapanma zamaný
    private bool orderPanelVisible = false;

    void Start()
    {
        orderPanel.SetActive(false); // OrderPanel'i baþlangýçta kapalý yap
    }

    void Update()
    {
        // Saat sistemine göre sipariþleri kontrol et
        if (Time.time >= nextOrderTime)
        {
            PlaceRandomOrder();
            nextOrderTime = Time.time + orderInterval;
        }

        // OrderPanel'i gösterme süresini kontrol et
        if (orderPanelVisible && Time.time >= orderDisplayEndTime)
        {
            orderPanel.SetActive(false);
            orderPanelVisible = false;
        }
    }

    void PlaceRandomOrder()
    {
        Dictionary<string, int> sellQuantities = sellPanel.GetSellQuantities();
        Dictionary<string, int> sellPrices = sellPanel.GetSellPrices();

        // En az 4 farklý ürün olmasý durumunda sipariþ verebiliriz
        if (sellQuantities.Count >= 4)
        {
            orderPanelText.text = "Bilgisayar Sipariþ Verdi:\n";
            orderPanel.SetActive(true); // OrderPanel'i aç
            orderDisplayEndTime = Time.time + orderDisplayDuration; // Kapanma zamanýný ayarla
            orderPanelVisible = true;

            List<string> productsToRemove = new List<string>();

            for (int i = 0; i < 4; i++)
            {
                int randomIndex = Random.Range(0, sellQuantities.Count);
                var item = new List<KeyValuePair<string, int>>(sellQuantities)[randomIndex];

                string productName = item.Key;
                int quantityToSell = item.Value;
                int price = sellPrices[productName];

                // Bilgisayarýn ürünü satýn almasý
                sellQuantities[productName]--;
                if (sellQuantities[productName] <= 0)
                {
                    productsToRemove.Add(productName);
                }

                playerBalance.AddBalance(price);
                orderPanelText.text += $"{productName} - {price} $\n";

                Debug.Log($"Bilgisayar {productName} sipariþ verdi! Yeni bakiye: {playerBalance.GetBalance()} $");
            }

            // Stoklarý güncelle
            foreach (string productName in productsToRemove)
            {
                sellQuantities.Remove(productName);
            }

            // Satýþ sonrasý paneli güncelle
            sellPanel.UpdateSellPanel();
        }
        else
        {
            Debug.Log("Yeterli ürün yok, en az 4 ürün satýþta olmalý.");
        }
    }
}
