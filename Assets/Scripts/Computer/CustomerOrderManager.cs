using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class CustomerOrderManager : MonoBehaviour
{
    public InGameMarket inGameMarket;
    public List<OrderData> activeOrders = new List<OrderData>();

    [Header("Sipariş Ayarları")]
    public int maxActiveOrders = 5;
    public float minOrderInterval = 120f;
    public float maxOrderInterval = 300f;

    private float orderTimer;

    public static event Action OnOrderListChanged;

    private void Start()
    {
        ResetOrderTimer();
    }

    private void Update()
    {
        orderTimer -= Time.deltaTime;
        if (orderTimer <= 0)
        {
            if (activeOrders.Count < maxActiveOrders)
            {
                GenerateAndDisplayNewOrder();
            }
            ResetOrderTimer();
        }

        CheckForExpiredOrders();
    }

    void ResetOrderTimer()
    {
        orderTimer = Random.Range(minOrderInterval, maxOrderInterval);
    }

    void GenerateAndDisplayNewOrder()
    {
        OrderData newOrder = GenerateNewOrder();
        if (newOrder != null)
        {
            activeOrders.Add(newOrder);
            OnOrderListChanged?.Invoke();
            Debug.Log($"Yeni sipariş oluşturuldu: {newOrder.orderID}");
        }
    }

    // Güncellenmiş Metot
    public OrderData GenerateNewOrder()
    {
        if (inGameMarket == null)
        {
            Debug.LogError("InGameMarket referansı CustomerOrderManager'da atanmamış!");
            return null;
        }

        OrderData newOrder = new OrderData();
        newOrder.InitializeCustomer();

        List<MarketProduct> availableProducts = new List<MarketProduct>();
        foreach (Category category in inGameMarket.productCategories)
        {
            // Sadece kategorinin kilidinin açık olup olmadığını kontrol ediyoruz.
            if (category.isUnlocked)
            {
                // Ürün bazlı lisans kontrolü kaldırıldı.
                // Kategori açıksa, içindeki tüm ürünler sipariş için uygun sayılır.
                availableProducts.AddRange(category.productsInCategory);
            }
        }

        if (availableProducts.Count == 0)
        {
            Debug.LogWarning("Sipariş oluşturulacak, oyuncunun lisansına sahip olduğu hiçbir kategori bulunamadı.");
            return null;
        }

        int itemsInOrder = Random.Range(1, 4);
        for (int i = 0; i < itemsInOrder; i++)
        {
            if (availableProducts.Count == 0) break;

            int randomIndex = Random.Range(0, availableProducts.Count);
            MarketProduct randomProduct = availableProducts[randomIndex];
            availableProducts.RemoveAt(randomIndex);

            int quantity = Random.Range(1, 5);
            int marketAverage = (int)randomProduct.currentAverageMarketPrice;
            newOrder.itemsInOrder.Add(new OrderItemDetail(randomProduct, quantity, randomProduct.price, marketAverage));
        }

        if (newOrder.itemsInOrder.Count == 0) return null;

        newOrder.timeLimit = 600f;

        return newOrder;
    }

    void CheckForExpiredOrders()
    {
        List<OrderData> expiredOrders = activeOrders
            .Where(order => (order.status == OrderStatus.Yeni || order.status == OrderStatus.Hazirlaniyor) &&
                            (DateTime.Now - order.orderTimestamp).TotalSeconds > order.timeLimit)
            .ToList();

        foreach (OrderData order in expiredOrders)
        {
            FailOrder(order);
        }
    }

    public void CompleteOrder(OrderData order)
    {
        order.status = OrderStatus.Completed;
        OnOrderListChanged?.Invoke();
    }

    public void FailOrder(OrderData order)
    {
        order.status = OrderStatus.Failed;
        OnOrderListChanged?.Invoke();
    }

    public List<OrderData> GetPendingOrders()
    {
        return activeOrders.Where(o => o.status == OrderStatus.Yeni || o.status == OrderStatus.Hazirlaniyor).ToList();
    }

    public void UpdateOrderStatus(string orderId, OrderStatus newStatus)
    {
        OrderData order = activeOrders.FirstOrDefault(o => o.orderID == orderId);
        if (order != null)
        {
            order.status = newStatus;
            OnOrderListChanged?.Invoke();
        }
    }
}