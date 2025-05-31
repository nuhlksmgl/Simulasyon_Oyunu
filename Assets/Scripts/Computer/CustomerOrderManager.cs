// CustomerOrderManager.cs (Debug Log'ları ve Küçük İyileştirmelerle)
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class CustomerOrderManager : MonoBehaviour
{
    [Header("Sipariş Sıklık Ayarları (İtibara Göre Değişir)")]
    [Tooltip("İtibar DÜŞÜKKEN bir sonraki sipariş denemesi için minimum bekleme süresi (GERÇEK saniye).")]
    public float minOrderIntervalLowRep = 180f;
    [Tooltip("İtibar DÜŞÜKKEN bir sonraki sipariş denemesi için maksimum bekleme süresi (GERÇEK saniye).")]
    public float maxOrderIntervalLowRep = 360f;
    [Tooltip("İtibar YÜKSEKKEN bir sonraki sipariş denemesi için minimum bekleme süresi (GERÇEK saniye).")]
    public float minOrderIntervalHighRep = 10f; // Daha agresif bir değer denenebilir
    [Tooltip("İtibar YÜKSEKKEN bir sonraki sipariş denemesi için maksimum bekleme süresi (GERÇEK saniye).")]
    public float maxOrderIntervalHighRep = 25f; // Daha agresif bir değer denenebilir

    private float currentMinOrderInterval;
    private float currentMaxOrderInterval;
    private float nextOrderAttemptTime;

    [Header("Sipariş Oluşma Olasılığı (İtibara Göre Değişir)")]
    [Tooltip("İtibar DÜŞÜKKEN, sipariş deneme zamanı geldiğinde sipariş oluşma olasılığı (0-1).")]
    [Range(0f, 1f)]
    public float orderChanceAtMinRep = 0.4f; // Biraz düşürülebilir, interval zaten uzun olacak
    [Tooltip("İtibar YÜKSEKKEN, sipariş deneme zamanı geldiğinde sipariş oluşma olasılığı (0-1).")]
    [Range(0f, 1f)]
    public float orderChanceAtMaxRep = 0.98f; // Yüksek tutalım
    private float currentOrderChance;

    [Header("Sipariş İçerik Ayarları")]
    public int minItemTypesPerOrder = 1;
    public int maxItemTypesPerOrder = 3;
    public int minQuantityPerItem = 1;
    public int maxQuantityPerItem = 3;
    [Tooltip("Bir ürünün, talep faktörü 1 iken, bir siparişe dahil edilme temel olasılığı (0-1).")]
    [Range(0f, 1f)]
    public float baseProductInclusionChance = 0.90f; // Yüksek tutalım

    [Header("Bağlantılar")]
    public InGameMarket inGameMarket;
    public SellPanel sellPanel;
    public TimeManager timeManager;
    public PackingStation packingStation;
    public StoreReputation storeReputation;

    [Header("Sipariş Listesi")]
    public List<OrderData> pendingOrders = new List<OrderData>();

    public static event System.Action OnOrderListChanged;

    void Start()
    {
        if (inGameMarket == null || sellPanel == null || timeManager == null || packingStation == null || storeReputation == null)
        {
            Debug.LogError("CustomerOrderManager: Gerekli script referansları atanmamış! Script devre dışı bırakılıyor.");
            enabled = false;
            return;
        }

        currentMinOrderInterval = minOrderIntervalLowRep;
        currentMaxOrderInterval = maxOrderIntervalLowRep;
        currentOrderChance = orderChanceAtMinRep;

        StoreReputation.OnReputationChanged += HandleReputationChange;

        if (StoreReputation.Instance != null)
        {
            HandleReputationChange(StoreReputation.Instance.GetCurrentReputation()); // Başlangıç ayarlarını yap
        }
        else // StoreReputation.Instance null ise, varsayılan düşük rep ayarlarıyla başla
        {
            AdjustOrderSettingsBasedOnReputation(0); // veya storeReputation.currentReputation
        }
        // CalculateNextOrderAttemptTime() zaten HandleReputationChange içinde çağrılıyor
    }

    void OnDestroy()
    {
        if (StoreReputation.Instance != null) // Güvenlik için null check
        {
            StoreReputation.OnReputationChanged -= HandleReputationChange;
        }
    }

    private void HandleReputationChange(float newReputation)
    {
        AdjustOrderSettingsBasedOnReputation(newReputation);
    }

    void AdjustOrderSettingsBasedOnReputation(float newReputation)
    {
        float reputationFactor = 0f;
        if (storeReputation != null && storeReputation.maxReputation > storeReputation.minReputation)
        {
            reputationFactor = Mathf.Clamp01((newReputation - storeReputation.minReputation) / (storeReputation.maxReputation - storeReputation.minReputation));
        }
        else if (storeReputation != null && newReputation >= storeReputation.maxReputation)
        {
            reputationFactor = 1f;
        }

        currentMinOrderInterval = Mathf.Lerp(minOrderIntervalLowRep, minOrderIntervalHighRep, reputationFactor);
        currentMaxOrderInterval = Mathf.Lerp(maxOrderIntervalLowRep, maxOrderIntervalHighRep, reputationFactor);
        currentOrderChance = Mathf.Lerp(orderChanceAtMinRep, orderChanceAtMaxRep, reputationFactor);

        Debug.Log($"İTİBAR GÜNCELLENDİ ({newReputation:F1}) -> Sip. Aralığı: {currentMinOrderInterval:F1}-{currentMaxOrderInterval:F1}s, Oluşma Şansı: {currentOrderChance:P0}");
        CalculateNextOrderAttemptTime(); // Yeni ayarlara göre bir sonraki deneme zamanını hemen hesapla
    }

    void Update()
    {
        if (Time.time >= nextOrderAttemptTime)
        {
            Debug.Log($"SİPARİŞ DENEME ZAMANI GELDİ. Şans: {currentOrderChance:P0}, Zar: {Random.value:F2}");
            if (Random.value < currentOrderChance)
            {
                Debug.Log("Sipariş oluşturma şansı YAKALANDI. GenerateNewOrder() çağrılıyor.");
                GenerateNewOrder();
            }
            else
            {
                Debug.Log("Sipariş oluşturma şansı YAKALANAMADI.");
            }
            CalculateNextOrderAttemptTime();
        }
    }

    void CalculateNextOrderAttemptTime()
    {
        float interval = Random.Range(currentMinOrderInterval, currentMaxOrderInterval);
        nextOrderAttemptTime = Time.time + interval;
        Debug.Log($"Bir sonraki sipariş DENEMESİ {interval:F1} saniye sonra (Gerçek zaman: {nextOrderAttemptTime:F1}).");
    }

    void GenerateNewOrder()
    {
        Debug.Log("GenerateNewOrder() BAŞLADI.");
        if (sellPanel.sellPrices == null || inGameMarket.products == null)
        {
            Debug.LogWarning("CustomerOrderManager: SellPanel.sellPrices veya InGameMarket.products henüz initialize edilmemiş.");
            return;
        }

        List<string> sellableProductNames = new List<string>(sellPanel.sellPrices.Keys);

        if (sellableProductNames.Count == 0 || sellableProductNames.Count < minItemTypesPerOrder)
        {
            Debug.LogWarning("Yeni sipariş için yeterli çeşitlilikte veya hiç satılabilir ürün yok.");
            return;
        }

        OrderData newOrder = new OrderData();
        // TimeManager.cs'de public float GetTotalMinutesPassedInGame() metodu olduğundan emin ol
        newOrder.orderTimestamp = timeManager.GetTotalMinutesPassedInGame();
        newOrder.timeMultiplierAtOrderCreation = timeManager.timeMultiplier;

        int numberOfItemTypesInThisOrder = Random.Range(minItemTypesPerOrder, Mathf.Min(maxItemTypesPerOrder, sellableProductNames.Count) + 1);
        List<string> chosenProductNamesForThisOrder = new List<string>();
        int itemsSuccessfullyAddedToOrder = 0;

        Debug.Log($"Oluşturulacak sipariş için {numberOfItemTypesInThisOrder} çeşit ürün denenecek.");

        for (int i = 0; i < numberOfItemTypesInThisOrder; i++)
        {
            if (chosenProductNamesForThisOrder.Count >= sellableProductNames.Count) break;

            string selectedProductName = null;
            int productNameSelectionAttempts = 0;
            bool foundUniqueProduct = false;

            while (productNameSelectionAttempts < sellableProductNames.Count * 3 && !foundUniqueProduct)
            {
                if (sellableProductNames.Count == chosenProductNamesForThisOrder.Count) break;
                string candidateProductName = sellableProductNames[Random.Range(0, sellableProductNames.Count)];
                if (!chosenProductNamesForThisOrder.Contains(candidateProductName))
                {
                    selectedProductName = candidateProductName;
                    foundUniqueProduct = true;
                }
                productNameSelectionAttempts++;
            }

            if (!foundUniqueProduct || selectedProductName == null)
            {
                Debug.Log("Bu iterasyon için benzersiz ürün bulunamadı, atlanıyor.");
                continue;
            }

            chosenProductNamesForThisOrder.Add(selectedProductName); // Seçildi olarak işaretle

            InGameMarket.MarketProduct productDefinition = FindProductDefinitionByName(selectedProductName);
            if (productDefinition == null) { Debug.LogError($"Ürün tanımı bulunamadı (GenerateNewOrder): {selectedProductName}."); continue; }

            int playerSellingPrice;
            if (!sellPanel.sellPrices.TryGetValue(selectedProductName, out playerSellingPrice)) { Debug.LogError($"{selectedProductName} için satış fiyatı SellPanel'de bulunamadı!"); continue; }

            int currentMarketPrice = (int)productDefinition.currentAverageMarketPrice;
            if (currentMarketPrice <= 0) currentMarketPrice = (int)productDefinition.baseMarketPrice;
            if (currentMarketPrice <= 0) currentMarketPrice = (int)(productDefinition.price * 1.5f);

            float priceRatio = (currentMarketPrice > 0) ? (float)playerSellingPrice / currentMarketPrice : 1.0f;
            float demandFactor = 1.0f;

            if (priceRatio > 1.20f) demandFactor = 0.10f;  // Daha da düşürüldü
            else if (priceRatio > 1.10f) demandFactor = 0.40f; // Biraz düşürüldü
            else if (priceRatio < 0.80f) demandFactor = 2.0f;  // Artırıldı
            else if (priceRatio < 0.95f) demandFactor = 1.5f;  // Artırıldı

            float effectiveInclusionChance = baseProductInclusionChance * demandFactor;
            // Debug.Log($"Ürün: {selectedProductName}, P.Fiyat: {playerSellingPrice}, Pys.Fiyat: {currentMarketPrice}, Oran: {priceRatio:F2}, TalepFaktörü: {demandFactor:F2}, EtkinEklemeŞansı: {effectiveInclusionChance:P0}");

            if (Random.value > effectiveInclusionChance)
            {
                Debug.Log($"{selectedProductName} talep faktörü ({demandFactor:F2}) ve şans ({baseProductInclusionChance:P0}) nedeniyle siparişe EKLENMEDİ (Zar: {Random.value:F2} > Etkin Şans: {effectiveInclusionChance:F2}).");
                continue;
            }

            int baseQuantity = Random.Range(minQuantityPerItem, maxQuantityPerItem + 1);
            int finalQuantity = Mathf.Clamp(Mathf.RoundToInt(baseQuantity * demandFactor), 1, maxQuantityPerItem * 3); // Üst limit biraz daha fazla esnetildi

            newOrder.itemsInOrder.Add(new OrderItemDetail(productDefinition, finalQuantity, playerSellingPrice, currentMarketPrice));
            newOrder.totalOrderValue += finalQuantity * playerSellingPrice;
            itemsSuccessfullyAddedToOrder++;
            Debug.Log($"{selectedProductName} (x{finalQuantity}) siparişe EKLENDİ.");
        }

        if (itemsSuccessfullyAddedToOrder == 0)
        {
            Debug.LogWarning("Siparişe talep koşulları nedeniyle HİÇBİR ürün eklenemedi, sipariş oluşturulmuyor.");
            return;
        }

        // Sipariş Tipi ve Teslim Süresi Ata (Mevcut mantık korunabilir)
        // ... (Kodun bu kısmı aynı kalabilir) ...
        float chanceForExpress = 0.3f;
        float chanceForSameDay = 0.1f;

        if (Random.value < chanceForSameDay && newOrder.itemsInOrder.Count <= 2)
        {
            newOrder.orderType = OrderType.AyniGun;
            newOrder.dueTimestamp = newOrder.orderTimestamp + (6 * 60);
        }
        else if (Random.value < chanceForExpress)
        {
            newOrder.orderType = OrderType.Express;
            newOrder.dueTimestamp = newOrder.orderTimestamp + (24 * 60);
        }
        else
        {
            newOrder.orderType = OrderType.Standart;
            newOrder.dueTimestamp = newOrder.orderTimestamp + (48 * 60);
        }

        pendingOrders.Add(newOrder);
        Debug.Log($"YENİ MÜŞTERİ SİPARİŞİ OLUŞTURULDU: ID {newOrder.orderID}, Müşteri: {newOrder.customerName}, Tip: {newOrder.orderType}, Çeşit: {newOrder.itemsInOrder.Count}, Değer: {newOrder.totalOrderValue}$ (İtibar: {storeReputation.GetCurrentReputation():F1}, Genel Sip. Şansı: {currentOrderChance:P0})");

        OnOrderListChanged?.Invoke();
    }

    InGameMarket.MarketProduct FindProductDefinitionByName(string name)
    {
        if (inGameMarket != null && inGameMarket.products != null)
        {
            return inGameMarket.products.FirstOrDefault(p => p.productName == name);
        }
        return null;
    }

    public List<OrderData> GetPendingOrders()
    {
        return pendingOrders;
    }

    public void UpdateOrderStatus(string orderID, OrderStatus newStatus)
    {
        OrderData orderToUpdate = pendingOrders.FirstOrDefault(o => o.orderID == orderID);
        if (orderToUpdate != null)
        {
            orderToUpdate.status = newStatus;
            Debug.Log($"Sipariş {orderID} durumu güncellendi: {newStatus}");
            OnOrderListChanged?.Invoke();
        }
    }
}