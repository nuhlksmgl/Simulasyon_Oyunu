using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class CustomerOrderManager : MonoBehaviour
{
    [Header("Sipariş Ayarları")]
    public float minOrderInterval = 60f;
    public float maxOrderInterval = 180f;
    private float nextOrderTriggerTime;

    [Header("Sipariş İçerik Ayarları")]
    public int minItemTypesPerOrder = 1;
    public int maxItemTypesPerOrder = 3;
    public int minQuantityPerItem = 1;
    public int maxQuantityPerItem = 3;

    [Header("Bağlantılar")]
    public InGameMarket inGameMarket;
    public SellPanel sellPanel;
    public TimeManager timeManager;
    public PackingStation packingStation; // ✅ EKLENDİ

    [Header("Sipariş Listesi")]
    public List<OrderData> pendingOrders = new List<OrderData>();

    void Start()
    {
        if (inGameMarket == null || sellPanel == null || timeManager == null || packingStation == null)
        {
            Debug.LogError("CustomerOrderManager: Gerekli referanslar atanmamış!");
            enabled = false;
            return;
        }

        CalculateNextOrderTime();
    }

    void Update()
    {
        if (Time.time >= nextOrderTriggerTime)
        {
            if (Random.value < 0.75f)
            {
                GenerateNewOrder();
            }
            CalculateNextOrderTime();
        }
    }

    void CalculateNextOrderTime()
    {
        float interval = Random.Range(minOrderInterval, maxOrderInterval);
        nextOrderTriggerTime = Time.time + interval;
    }

    void GenerateNewOrder()
    {
        List<string> sellableProductNames = new List<string>(sellPanel.sellPrices.Keys);

        if (sellableProductNames.Count < minItemTypesPerOrder)
            return;

        OrderData newOrder = new OrderData();
        newOrder.orderTimestamp = timeManager.currentHour * 60f + timeManager.currentMinute;
        newOrder.timeMultiplierAtOrderCreation = timeManager.timeMultiplier;

        int numberOfItemTypes = Random.Range(minItemTypesPerOrder, Mathf.Min(maxItemTypesPerOrder, sellableProductNames.Count) + 1);
        List<string> chosenProductNames = new List<string>();

        for (int i = 0; i < numberOfItemTypes; i++)
        {
            if (chosenProductNames.Count >= sellableProductNames.Count) break;

            string randomProductName;
            do
            {
                randomProductName = sellableProductNames[Random.Range(0, sellableProductNames.Count)];
            } while (chosenProductNames.Contains(randomProductName));

            chosenProductNames.Add(randomProductName);

            InGameMarket.MarketProduct productDefinition = FindProductDefinitionByName(randomProductName);
            if (productDefinition == null)
            {
                Debug.LogError($"Ürün bulunamadı: {randomProductName}");
                continue;
            }

            int quantity = Random.Range(minQuantityPerItem, maxQuantityPerItem + 1);
            int price = sellPanel.sellPrices[randomProductName];
            newOrder.itemsInOrder.Add(new OrderItemDetail(productDefinition, quantity, price));
            newOrder.totalOrderValue += quantity * price;
        }

        if (newOrder.itemsInOrder.Count == 0) return;

        pendingOrders.Add(newOrder);

        // ✅ Kutu oluşturma işlemini PackingStation'a devret
        packingStation.SpawnCargoBoxForOrder(newOrder);
    }

    InGameMarket.MarketProduct FindProductDefinitionByName(string name)
    {
        return inGameMarket.products.FirstOrDefault(p => p.productName == name);
    }

    public List<OrderData> GetPendingOrders()
    {
        return pendingOrders;
    }
}
