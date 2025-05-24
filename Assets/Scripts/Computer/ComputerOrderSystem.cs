using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class ComputerOrderSystem : MonoBehaviour
{
    public InGameMarket inGameMarket;
    public PlayerBalance playerBalance;
    public SellPanel sellPanel;

    public float orderInterval = 10.0f;
    private float nextOrderTime = 0.0f;

    public TextMeshProUGUI orderPanelText;
    public GameObject orderPanel;
    private float orderDisplayDuration = 60.0f;
    private float orderDisplayEndTime = 0.0f;
    private bool orderPanelVisible = false;

    void Start()
    {
        orderPanel.SetActive(false);
    }

    void Update()
    {
        if (Time.time >= nextOrderTime)
        {
            PlaceRandomOrder();
            nextOrderTime = Time.time + orderInterval;
        }

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

        if (sellQuantities.Count >= 4)
        {
            orderPanelText.text = "Bilgisayar Sipariþi Verdi:\n";
            orderPanel.SetActive(true);
            orderDisplayEndTime = Time.time + orderDisplayDuration;
            orderPanelVisible = true;

            List<string> productsToRemove = new List<string>();

            for (int i = 0; i < 4; i++)
            {
                int randomIndex = Random.Range(0, sellQuantities.Count);
                var item = new List<KeyValuePair<string, int>>(sellQuantities)[randomIndex];

                string productName = item.Key;
                int quantityToSell = item.Value;

                if (!sellPrices.TryGetValue(productName, out int price))
                {
                    Debug.LogWarning($"{productName} için fiyat bulunamadý.");
                    continue;
                }

                sellQuantities[productName]--;
                if (sellQuantities[productName] <= 0)
                {
                    productsToRemove.Add(productName);
                }

                playerBalance.AddBalance(price);
                orderPanelText.text += $"{productName} - {price} $\n";

                Debug.Log($"Bilgisayar {productName} sipariþi verdi. Yeni bakiye: {playerBalance.GetBalance()}");
            }

            foreach (string productName in productsToRemove)
            {
                sellQuantities.Remove(productName);
            }

            sellPanel.UpdateSellPanel();
        }
    }
}
