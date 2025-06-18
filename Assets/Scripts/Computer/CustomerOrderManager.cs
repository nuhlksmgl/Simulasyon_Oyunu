// FileName: CustomerOrderManager.cs
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class CustomerOrderManager : MonoBehaviour
{
    public static CustomerOrderManager Instance { get; private set; }

    [Header("Bağlantılar")]
    public InGameMarket inGameMarket;
    public StoreReputation storeReputation;
    public PlayerBalance playerBalance;

    [Header("Sipariş Sıklık Ayarları")]
    public float baseMinOrderInterval = 90f;
    public float baseMaxOrderInterval = 240f;

    [Header("İtibar Etkileri - Sıklık")]
    public float slowdownFactorAtMinRep = 2.0f;
    public float speedupFactorAtMaxRep = 0.2f;

    [Header("Sipariş İçerik Ayarları")]
    public int maxActiveOrders = 5;
    [Range(0f, 1f)]
    public float baseInclusionChance = 0.4f;
    [Range(0f, 1f)]
    public float maxAcceptablePriceMargin = 0.25f;

    [Header("İtibar Etkileri - Kalite")]
    public float reputationForSuccess = 1.5f;
    public float reputationForFailure = -3.0f;

    public List<OrderData> activeOrders = new List<OrderData>();
    private float orderTimer;
    public static event Action OnOrderListChanged;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(this.gameObject); }
        else { Instance = this; }
    }

    void Start()
    {
        if (inGameMarket == null) Debug.LogError("CustomerOrderManager: InGameMarket referansı eksik!");
        if (storeReputation == null) storeReputation = FindObjectOfType<StoreReputation>();
        if (playerBalance == null) playerBalance = FindObjectOfType<PlayerBalance>();
        ResetOrderTimer();
    }

    void Update()
    {
        orderTimer -= Time.deltaTime;
        if (orderTimer <= 0)
        {
            if (activeOrders.Count < maxActiveOrders) GenerateNewOrder();
            ResetOrderTimer();
        }
        CheckForExpiredOrders();
    }

    public void ProcessShippedOrder(CargoBox shippedBox)
    {
        if (shippedBox == null || shippedBox.assignedOrder == null) return;
        OrderData order = shippedBox.assignedOrder;
        float packingPenalty = shippedBox.CalculatePackingPenalty();
        if (packingPenalty == 0)
        {
            Debug.Log($"Sipariş {order.orderID} BAŞARILI!");
            if (storeReputation != null) storeReputation.AddReputation(reputationForSuccess);
            if (playerBalance != null) playerBalance.AddBalance(order.totalOrderValue);
            CompleteOrder(order.orderID);
        }
        else
        {
            Debug.LogWarning($"Sipariş {order.orderID} HATALI! İtibar Cezası: {packingPenalty}");
            if (storeReputation != null) storeReputation.AddReputation(packingPenalty);
            FailOrder(order.orderID, applyPenalty: false);
        }
        if (ActiveOrderManager.Instance != null && ActiveOrderManager.Instance.activeOrder?.orderID == order.orderID)
        {
            ActiveOrderManager.Instance.ClearActiveOrder();
        }
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
                    item.productDefinition.physicalStock -= item.quantity;
            }
            OnOrderListChanged?.Invoke();
        }
    }

    public void FailOrder(string orderId, bool applyPenalty = true)
    {
        OrderData order = activeOrders.FirstOrDefault(o => o.orderID == orderId);
        if (order != null && (order.status == OrderStatus.Yeni || order.status == OrderStatus.Hazirlaniyor))
        {
            order.status = OrderStatus.Failed;
            if (applyPenalty && storeReputation != null)
                storeReputation.AddReputation(reputationForFailure);
            OnOrderListChanged?.Invoke();
        }
    }

    private void CheckForExpiredOrders()
    {
        var expired = activeOrders.Where(o => (o.status == OrderStatus.Yeni || o.status == OrderStatus.Hazirlaniyor) && (DateTime.Now - o.orderTimestamp).TotalSeconds > o.timeLimit).ToList();
        foreach (var order in expired)
        {
            Debug.LogWarning($"Sipariş {order.orderID} süresi dolduğu için iptal edildi!");
            FailOrder(order.orderID, true);
        }
    }

    #region Diğer Metodlar
    public void UpdateOrderStatus(string orderId, OrderStatus newStatus)
    {
        OrderData order = activeOrders.FirstOrDefault(o => o.orderID == orderId);
        if (order != null)
        {
            order.status = newStatus;
            OnOrderListChanged?.Invoke();
        }
    }

    public List<OrderData> GetPendingOrders() => activeOrders.Where(o => o.status == OrderStatus.Yeni || o.status == OrderStatus.Hazirlaniyor).ToList();

    private void ResetOrderTimer()
    {
        float repPercent = (storeReputation != null) ? (storeReputation.GetCurrentReputation() / storeReputation.maxReputation) : 0f;
        float multiplier = Mathf.Lerp(slowdownFactorAtMinRep, speedupFactorAtMaxRep, repPercent);
        orderTimer = Random.Range(baseMinOrderInterval * multiplier, baseMaxOrderInterval * multiplier);
    }

    private void GenerateNewOrder()
    {
        if (inGameMarket == null) return;
        var products = inGameMarket.GetAllUnlockedProducts().Where(p => p.physicalStock > 0 && p.price > 0 && p.isListedForSale).ToList();
        if (products.Count == 0) return;
        OrderData newOrder = new OrderData();
        newOrder.InitializeCustomer();
        float repPercent = (storeReputation != null) ? (storeReputation.GetCurrentReputation() / storeReputation.maxReputation) : 0.5f;
        int maxItems = Mathf.RoundToInt(Mathf.Lerp(1, 4, repPercent));
        foreach (var p in products)
        {
            if (newOrder.itemsInOrder.Count >= maxItems) break;
            if (newOrder.itemsInOrder.Any(i => i.productDefinition == p)) continue;
            float marketPrice = p.currentAverageMarketPrice;
            if (marketPrice <= 0 || p.price > marketPrice * (1 + maxAcceptablePriceMargin)) continue;
            float priceRatio = p.price / marketPrice;
            float demand = 1.0f;
            if (priceRatio > 1.15f) demand = 0.2f;
            else if (priceRatio > 1.0f) demand = 0.7f;
            else if (priceRatio < 0.85f) demand = 2.0f; else if (priceRatio < 1.0f) demand = 1.5f;
            if (Random.value < (baseInclusionChance * demand))
            {
                int qty = Mathf.Clamp(Mathf.RoundToInt(Random.Range(1, 4) * demand), 1, p.physicalStock);
                if (qty > 0) newOrder.itemsInOrder.Add(new OrderItemDetail(p, qty, p.price, (int)marketPrice));
            }
        }
        if (newOrder.itemsInOrder.Count > 0)
        {
            newOrder.timeLimit = 600f;
            activeOrders.Add(newOrder);
            OnOrderListChanged?.Invoke();
            Debug.Log($"YENİ SİPARİŞ: ID {newOrder.orderID}, Çeşit: {newOrder.itemsInOrder.Count}");
        }
    }
    #endregion
}