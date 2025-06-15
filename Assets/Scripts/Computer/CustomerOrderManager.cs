using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class CustomerOrderManager : MonoBehaviour
{
    [Header("Bağlantılar")]
    public InGameMarket inGameMarket;
    public StoreReputation storeReputation;

    // --- DEĞİŞİKLİK: ZAMANLAMA AYARLARI YENİDEN DÜZENLENDİ ---
    [Header("Sipariş Sıklık Ayarları")]
    [Tooltip("Siparişler arasındaki temel bekleme süresi aralığı (saniye).")]
    public float minInterval = 90f;
    public float maxInterval = 240f;

    [Header("İtibar Etkileri - Sıklık")]
    [Tooltip("İtibar SIFIRKEN bekleme süresi ne kadar yavaşlasın? (1.0 = normal, 2.0 = 2 kat yavaş)")]
    public float slowdownFactorAtMinRep = 2.0f;
    [Tooltip("İtibar MAKSİMUMKEN bekleme süresi ne kadar hızlansın? (1.0 = normal, 0.2 = 5 kat hızlı)")]
    public float speedupFactorAtMaxRep = 0.2f; // Bu değeri düşürerek hızı artırabilirsiniz

    [Header("İtibar Etkileri - Kalite")]
    public float reputationForSuccess = 1.5f;
    public float reputationForFailure = -3.0f;

    [Header("Sipariş İçerik Ayarları")]
    public int maxActiveOrders = 5;
    [Tooltip("Bir siparişe eklenecek ürün çeşidi için temel şans (0-1). Talep faktörü bunu etkiler.")]
    [Range(0f, 1f)]
    public float baseInclusionChance = 0.4f;
    [Tooltip("Müşterinin, bir ürünü piyasa fiyatının en fazla yüzde kaç fazlasına almayı kabul edeceği.")]
    [Range(0f, 1f)]
    public float maxAcceptablePriceMargin = 0.25f;

    public List<OrderData> activeOrders = new List<OrderData>();
    private float orderTimer;
    public static event Action OnOrderListChanged;

    void Start()
    {
        if (inGameMarket == null) Debug.LogError("CustomerOrderManager: InGameMarket referansı eksik!");
        if (storeReputation == null) storeReputation = FindObjectOfType<StoreReputation>();
        ResetOrderTimer();
    }

    void Update()
    {
        orderTimer -= Time.deltaTime;
        if (orderTimer <= 0)
        {
            if (activeOrders.Count < maxActiveOrders)
            {
                GenerateNewOrder();
            }
            ResetOrderTimer();
        }
        CheckForExpiredOrders();
    }

    // GÜNCELLENMİŞ ZAMANLAYICI MANTIĞI
    void ResetOrderTimer()
    {
        float reputationPercent = (storeReputation != null) ? (storeReputation.GetCurrentReputation() / storeReputation.maxReputation) : 0f;

        // Düşük itibardaki yavaşlatma faktöründen, yüksek itibardaki hızlandırma faktörüne doğru bir değer hesapla
        float timeMultiplier = Mathf.Lerp(slowdownFactorAtMinRep, speedupFactorAtMaxRep, reputationPercent);

        float currentMinInterval = minInterval * timeMultiplier;
        float currentMaxInterval = maxInterval * timeMultiplier;

        orderTimer = Random.Range(currentMinInterval, currentMaxInterval);
    }

    public void GenerateNewOrder()
    {
        if (inGameMarket == null) return;

        var sellableProducts = inGameMarket.GetAllUnlockedProducts()
            .Where(p => p.physicalStock > 0 && p.price > 0).ToList();

        if (sellableProducts.Count == 0) return;

        OrderData newOrder = new OrderData();
        newOrder.InitializeCustomer();

        float reputationPercent = (storeReputation != null) ? (storeReputation.GetCurrentReputation() / storeReputation.maxReputation) : 0.5f;
        int maxItemTypesInOrder = Mathf.RoundToInt(Mathf.Lerp(1, 4, reputationPercent));

        foreach (var product in sellableProducts)
        {
            if (newOrder.itemsInOrder.Count >= maxItemTypesInOrder) break;
            if (newOrder.itemsInOrder.Any(item => item.productDefinition == product)) continue;

            float marketPrice = product.currentAverageMarketPrice;
            if (marketPrice <= 0 || product.price > marketPrice * (1 + maxAcceptablePriceMargin)) continue;

            float priceRatio = product.price / marketPrice;
            float demandFactor = 1.0f;

            if (priceRatio > 1.15f) demandFactor = 0.2f;
            else if (priceRatio > 1.0f) demandFactor = 0.7f;
            else if (priceRatio < 0.85f) demandFactor = 2.0f;
            else if (priceRatio < 1.0f) demandFactor = 1.5f;

            if (Random.value < (baseInclusionChance * demandFactor))
            {
                int baseQuantity = Random.Range(1, 4);
                int finalQuantity = Mathf.Clamp(Mathf.RoundToInt(baseQuantity * demandFactor), 1, product.physicalStock);
                if (finalQuantity > 0)
                {
                    newOrder.itemsInOrder.Add(new OrderItemDetail(product, finalQuantity, product.price, (int)marketPrice));
                }
            }
        }

        if (newOrder.itemsInOrder.Count > 0)
        {
            newOrder.timeLimit = 600f;
            activeOrders.Add(newOrder);
            OnOrderListChanged?.Invoke();
        }
    }

    void CheckForExpiredOrders()
    {
        var expiredOrders = activeOrders.Where(order =>
            (order.status == OrderStatus.Yeni || order.status == OrderStatus.Hazirlaniyor) &&
            (DateTime.Now - order.orderTimestamp).TotalSeconds > order.timeLimit
        ).ToList();
        foreach (var order in expiredOrders) { FailOrder(order.orderID); }
    }

    public void CompleteOrder(string orderId)
    {
        OrderData order = activeOrders.FirstOrDefault(o => o.orderID == orderId);
        if (order != null && (order.status == OrderStatus.Yeni || order.status == OrderStatus.Hazirlaniyor))
        {
            order.status = OrderStatus.Completed;
            foreach (var item in order.itemsInOrder)
            {
                if (item.productDefinition.physicalStock >= item.quantity)
                {
                    item.productDefinition.physicalStock -= item.quantity;
                }
            }
            storeReputation?.AddReputation(reputationForSuccess);
            OnOrderListChanged?.Invoke();
        }
    }

    public void FailOrder(string orderId)
    {
        OrderData order = activeOrders.FirstOrDefault(o => o.orderID == orderId);
        if (order != null && (order.status == OrderStatus.Yeni || order.status == OrderStatus.Hazirlaniyor))
        {
            order.status = OrderStatus.Failed;
            storeReputation?.AddReputation(reputationForFailure);
            OnOrderListChanged?.Invoke();
        }
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