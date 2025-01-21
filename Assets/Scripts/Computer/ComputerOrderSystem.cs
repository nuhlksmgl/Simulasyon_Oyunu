using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class ComputerOrderSystem : MonoBehaviour
{
    public InGameMarket inGameMarket; // InGameMarket script referans�
    public PlayerBalance playerBalance; // PlayerBalance script referans�
    public SellPanel sellPanel; // SellPanel script referans�
    public float orderInterval = 10.0f; // Sipari� verme aral���
    private float nextOrderTime = 0.0f;
    public TextMeshProUGUI orderPanelText; // Sipari�leri g�sterecek TextMeshPro UI eleman�
    public GameObject orderPanel; // Sipari� paneli
    private float orderDisplayDuration = 60.0f; // Panelin a��k kalma s�resi
    private float orderDisplayEndTime = 0.0f; // Panelin kapanma zaman�
    private bool orderPanelVisible = false;

    void Start()
    {
        orderPanel.SetActive(false); // OrderPanel'i ba�lang��ta kapal� yap
    }

    void Update()
    {
        // Saat sistemine g�re sipari�leri kontrol et
        if (Time.time >= nextOrderTime)
        {
            PlaceRandomOrder();
            nextOrderTime = Time.time + orderInterval;
        }

        // OrderPanel'i g�sterme s�resini kontrol et
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

        // En az 4 farkl� �r�n olmas� durumunda sipari� verebiliriz
        if (sellQuantities.Count >= 4)
        {
            orderPanelText.text = "Bilgisayar Sipari� Verdi:\n";
            orderPanel.SetActive(true); // OrderPanel'i a�
            orderDisplayEndTime = Time.time + orderDisplayDuration; // Kapanma zaman�n� ayarla
            orderPanelVisible = true;

            List<string> productsToRemove = new List<string>();

            for (int i = 0; i < 4; i++)
            {
                int randomIndex = Random.Range(0, sellQuantities.Count);
                var item = new List<KeyValuePair<string, int>>(sellQuantities)[randomIndex];

                string productName = item.Key;
                int quantityToSell = item.Value;
                int price = sellPrices[productName];

                // Bilgisayar�n �r�n� sat�n almas�
                sellQuantities[productName]--;
                if (sellQuantities[productName] <= 0)
                {
                    productsToRemove.Add(productName);
                }

                playerBalance.AddBalance(price);
                orderPanelText.text += $"{productName} - {price} $\n";

                Debug.Log($"Bilgisayar {productName} sipari� verdi! Yeni bakiye: {playerBalance.GetBalance()} $");
            }

            // Stoklar� g�ncelle
            foreach (string productName in productsToRemove)
            {
                sellQuantities.Remove(productName);
            }

            // Sat�� sonras� paneli g�ncelle
            sellPanel.UpdateSellPanel();
        }
        else
        {
            Debug.Log("Yeterli �r�n yok, en az 4 �r�n sat��ta olmal�.");
        }
    }
}
