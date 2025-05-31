using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;

public class InGameMarket : MonoBehaviour
{
    [System.Serializable]
    public class MarketProduct
    {
        public string productName;
        public int price;                   // Oyuncunun toptancıdan ALIŞ fiyatı
        public GameObject productPrefab;    // Ürünün spawn edilecek ANA prefab'ı
        public int quantity = 0;            // Oyuncunun bu üründen satılabilir genel stoğu
        public bool isLarge;
        public TextMeshProUGUI priceText;   // Toptancı UI'ında ALIŞ fiyatını gösteren Text

        // --- PİYASA FİYATI VE TREND ALANLARI ---
        [Header("Piyasa Fiyatı (Satış İçin)")]
        [Tooltip("Ürünün temel, ortalama piyasa SATIŞ fiyatı. Inspector'dan ayarlanabilir veya alış fiyatından hesaplanabilir.")]
        public float baseMarketPrice;
        [Tooltip("Güncel, dalgalanabilen ortalama piyasa SATIŞ fiyatı. MarketDynamicsManager tarafından güncellenir.")]
        public float currentAverageMarketPrice;
        [HideInInspector] // Kodla yönetileceği için Inspector'da görünmesine gerek yok
        public int priceTrendStreak = 0; // Pozitif: artış serisi, Negatif: düşüş serisi, 0: nötr veya kırılmış seri
        // --- PİYASA FİYATI VE TREND ALANLARI SONU ---
    }

    public MarketProduct[] products;
    public Transform[] spawnSlots; // Toptancıdan alınan kutuların spawn olacağı belirli slotlar
    public PlayerBalance playerBalance;
    public SellPanel sellPanel;

    [Header("Small Cargo Boxes Prefabs")] // İsimleri Prefab olarak netleştirdim
    public GameObject smallBox1SlotPrefab;
    public GameObject smallBox2SlotsPrefab;
    public GameObject smallBox3to4SlotsPrefab;

    [Header("Large Cargo Boxes Prefabs")]
    public GameObject largeBox1SlotPrefab;
    public GameObject largeBox2SlotsPrefab;
    public GameObject largeBox3to4SlotsPrefab;

    private List<GameObject> spawnedDeliveryBoxes = new List<GameObject>(); // Toptancı teslimat kutuları
    private List<OrderItemDetail> purchaseBasket = new List<OrderItemDetail>(); // Toptancıdan alım sepeti (OrderData.OrderItemDetail kullanıyor)

    void Start()
    {
        InitializeAllProductMarketPrices(); // Piyasa fiyatlarını ve trendleri başlangıçta ayarla
        SetupBuyButtons();
        UpdatePriceUI_BuyPanel(); // Toptancı UI'ındaki ALIŞ fiyatlarını güncelle
    }

    public void InitializeAllProductMarketPrices()
    {
        Debug.Log("InGameMarket: Tüm ürünlerin piyasa fiyatları initialize ediliyor...");
        if (products == null)
        {
            Debug.LogError("InGameMarket: 'products' dizisi null!");
            return;
        }

        foreach (MarketProduct product in products)
        {
            if (product == null)
            {
                Debug.LogWarning("InGameMarket: 'products' dizisinde null bir ürün referansı var.");
                continue;
            }

            // 1. baseMarketPrice'ı ayarla
            if (product.baseMarketPrice <= 0)
            {
                if (product.price > 0) // Alış fiyatı geçerliyse
                {
                    product.baseMarketPrice = Mathf.Round(product.price * 1.5f); // Örnek: Alış fiyatının %50 fazlası
                    // Debug.LogWarning($"{product.productName} için baseMarketPrice Inspector'dan ayarlanmamış veya 0. Alış fiyatından ({product.price}$) hesaplandı: {product.baseMarketPrice}$");
                }
                else // Alış fiyatı da geçersizse, varsayılan bir değer ata
                {
                    product.baseMarketPrice = 50f; // Güvenlik için varsayılan bir değer
                    Debug.LogError($"{product.productName} için hem baseMarketPrice hem de alış fiyatı geçersiz! baseMarketPrice varsayılan olarak {product.baseMarketPrice}$ ayarlandı.");
                }
            }

            // 2. currentAverageMarketPrice'ı baseMarketPrice'a eşitle (eğer başlangıçta 0 ise veya base'den çok farklıysa)
            if (product.currentAverageMarketPrice <= 0 || Mathf.Approximately(product.currentAverageMarketPrice, 0))
            {
                product.currentAverageMarketPrice = product.baseMarketPrice;
            }
            product.priceTrendStreak = 0; // Başlangıçta trend yok
            Debug.Log($"INIT: {product.productName} -> Alış: {product.price}$, Taban Piyasa: {product.baseMarketPrice:F0}$, Güncel Piyasa: {product.currentAverageMarketPrice:F0}$");
        }
    }

    public void SetupBuyButtons()
    {
        Button[] buttons = GetComponentsInChildren<Button>(true);
        int productIndexAssigned = 0;
        for (int i = 0; i < buttons.Length; i++)
        {
            if (buttons[i].CompareTag("BuyButton"))
            {
                if (productIndexAssigned < products.Length)
                {
                    int capturedProductIndex = productIndexAssigned;
                    buttons[i].onClick.RemoveAllListeners();
                    buttons[i].onClick.AddListener(() => AddToPurchaseBasket(capturedProductIndex));
                    productIndexAssigned++;
                }
                else
                {
                    buttons[i].gameObject.SetActive(false);
                }
            }
        }
    }

    void OnValidate() // Editörde değerler değiştiğinde UI'ı güncellemek için
    {
        if (!Application.isPlaying && products != null)
        {
            UpdatePriceUI_BuyPanel();
        }
    }

    public void AddToPurchaseBasket(int productIndex)
    {
        if (productIndex < 0 || productIndex >= products.Length) return;
        MarketProduct selectedProduct = products[productIndex];
        if (playerBalance == null) return;

        int quantityToBuy = 1;
        int costOfPurchase = selectedProduct.price * quantityToBuy;

        if (playerBalance.DeductBalance(costOfPurchase))
        {
            OrderItemDetail existingItemInBasket = purchaseBasket.Find(item => item.productDefinition == selectedProduct);
            if (existingItemInBasket != null)
            {
                existingItemInBasket.quantity += quantityToBuy;
            }
            else
            {
                // Toptancı sepeti için OrderItemDetail oluştururken, marketPriceAtOrderTime için güncel piyasa fiyatını kullanabiliriz.
                purchaseBasket.Add(new OrderItemDetail(selectedProduct, quantityToBuy, selectedProduct.price, (int)selectedProduct.currentAverageMarketPrice));
            }
            selectedProduct.quantity += quantityToBuy;
            Debug.Log($"{selectedProduct.productName} (Adet: {quantityToBuy}) toptancı sepetine eklendi. Oyuncunun bu üründen stoğu: {selectedProduct.quantity}");

            if (sellPanel != null)
            {
                sellPanel.UpdateSellPanel();
            }
        }
        else
        {
            Debug.LogWarning($"{selectedProduct.productName} için yetersiz bakiye!");
        }
    }

    public void ProcessPurchaseBasket()
    {
        if (purchaseBasket.Count == 0) return;
        Debug.Log("Toptancı siparişleri (satın alımlar) işleniyor...");
        foreach (OrderItemDetail itemDetailInBasket in purchaseBasket)
        {
            SpawnProductsInDeliveryBox(itemDetailInBasket.productDefinition, itemDetailInBasket.quantity);
        }
        purchaseBasket.Clear();
        Debug.Log("Toptancı siparişleri işlendi, satın alma sepeti temizlendi.");
        if (sellPanel != null) sellPanel.UpdateSellPanel();
    }

    void SpawnProductsInDeliveryBox(MarketProduct product, int quantity)
    {
        if (product == null || product.productPrefab == null)
        {
            Debug.LogError($"SpawnProductsInDeliveryBox: Ürün veya ürün prefabı null! Ürün: {(product?.productName ?? "NULL")}");
            return;
        }

        GameObject boxPrefabToSpawn = SelectCargoBoxPrefab(product.isLarge, quantity);
        if (boxPrefabToSpawn == null)
        {
            Debug.LogError($"Uygun kargo kutusu prefabı bulunamadı! Ürün: {product.productName}, Adet: {quantity}");
            return;
        }

        int slotIndex = GetAvailableDeliverySlotIndex();
        if (slotIndex == -1)
        {
            Debug.LogWarning("Boş teslimat slotu yok, toptancı kutusu spawn edilemedi!");
            return;
        }

        Transform selectedSpawnSlot = spawnSlots[slotIndex];
        GameObject boxInstance = Instantiate(boxPrefabToSpawn, selectedSpawnSlot.position, boxPrefabToSpawn.transform.rotation);
        spawnedDeliveryBoxes.Add(boxInstance);

        CargoBox cargoBoxScript = boxInstance.GetComponent<CargoBox>();
        if (cargoBoxScript == null)
        {
            Debug.LogError($"Kargo kutusu prefabı ({boxPrefabToSpawn.name}) üzerinde CargoBox scripti eksik!");
            Destroy(boxInstance);
            RemoveBoxFromSpawnedList(boxInstance);
            return;
        }

        OrderData supplierDeliveryOrder = new OrderData();
        supplierDeliveryOrder.InitializeCustomer();
        supplierDeliveryOrder.customerName = $"Tedarikçi - {product.productName}";
        supplierDeliveryOrder.itemsInOrder.Add(new OrderItemDetail(product, quantity, product.price, (int)product.currentAverageMarketPrice));
        supplierDeliveryOrder.totalOrderValue = product.price * quantity;
        supplierDeliveryOrder.status = OrderStatus.TeslimEdildi;
        cargoBoxScript.AssignOrder(supplierDeliveryOrder);

        for (int i = 0; i < quantity; i++)
        {
            GameObject productObjInstance = Instantiate(product.productPrefab, cargoBoxScript.transform.position, product.productPrefab.transform.rotation);
            Product productScriptComponent = productObjInstance.GetComponent<Product>();
            if (productScriptComponent != null)
            {
                productScriptComponent.productDefinition = product; // Product script'ine MarketProduct tanımını ata
                if (!cargoBoxScript.TryPlaceProduct(productScriptComponent))
                {
                    Debug.LogError($"❌ {product.productName} (adet {i + 1}) {boxInstance.name} kutusuna yerleştirilemedi!");
                    Destroy(productObjInstance);
                }
            }
            else
            {
                Debug.LogError($"❌ Prefab {product.productPrefab.name} içinde Product scripti yok!");
                Destroy(productObjInstance);
            }
        }
    }

    public GameObject SpawnEmptyOrderBoxForCustomer(OrderData customerOrder, Transform spawnTransform)
    {
        if (customerOrder == null || customerOrder.itemsInOrder == null || customerOrder.itemsInOrder.Count == 0)
        {
            Debug.LogError("SpawnEmptyOrderBoxForCustomer: Geçersiz veya boş müşteri siparişi!");
            return null;
        }

        int totalQuantityInOrder = 0;
        bool orderContainsLargeItem = false;
        foreach (var item in customerOrder.itemsInOrder)
        {
            if (item.productDefinition == null)
            {
                Debug.LogError($"SpawnEmptyOrderBoxForCustomer: Sipariş ({customerOrder.orderID}) içindeki bir ürünün tanımı null!");
                return null;
            }
            totalQuantityInOrder += item.quantity;
            if (item.productDefinition.isLarge)
                orderContainsLargeItem = true;
        }

        GameObject boxPrefabToSpawn = SelectCargoBoxPrefab(orderContainsLargeItem, totalQuantityInOrder);
        if (boxPrefabToSpawn == null)
        {
            Debug.LogError($"Müşteri siparişi ({customerOrder.orderID}) için uygun kargo kutusu prefabı bulunamadı!");
            return null;
        }

        GameObject boxInstance = Instantiate(boxPrefabToSpawn, spawnTransform.position, spawnTransform.rotation);
        CargoBox cargoBoxScript = boxInstance.GetComponent<CargoBox>();
        if (cargoBoxScript != null)
        {
            cargoBoxScript.AssignOrder(customerOrder);
            return boxInstance;
        }
        else
        {
            Debug.LogError($"Kargo kutusu prefabı ({boxPrefabToSpawn.name}) üzerinde CargoBox scripti eksik! Müşteri siparişi için kutu oluşturulamadı.");
            Destroy(boxInstance);
            return null;
        }
    }

    GameObject SelectCargoBoxPrefab(bool isLarge, int quantity)
    {
        if (isLarge)
        {
            if (quantity == 1 && largeBox1SlotPrefab != null) return largeBox1SlotPrefab;
            if (quantity == 2 && largeBox2SlotsPrefab != null) return largeBox2SlotsPrefab;
            if (quantity >= 3 && quantity <= 4 && largeBox3to4SlotsPrefab != null) return largeBox3to4SlotsPrefab;
        }
        else
        {
            if (quantity == 1 && smallBox1SlotPrefab != null) return smallBox1SlotPrefab;
            if (quantity == 2 && smallBox2SlotsPrefab != null) return smallBox2SlotsPrefab;
            if (quantity >= 3 && quantity <= 4 && smallBox3to4SlotsPrefab != null) return smallBox3to4SlotsPrefab;
        }
        Debug.LogWarning($"Uygun kargo kutusu prefabı bulunamadı. Büyük mü: {isLarge}, Adet: {quantity}. Lütfen Inspector'da prefabları kontrol edin.");
        return null;
    }

    int GetAvailableDeliverySlotIndex()
    {
        for (int i = 0; i < spawnSlots.Length; i++)
        {
            if (spawnSlots[i] == null) continue;
            bool isOccupied = false;
            Collider[] collidersInSlot = Physics.OverlapSphere(spawnSlots[i].position, 0.2f);
            foreach (Collider col in collidersInSlot)
            {
                if (spawnedDeliveryBoxes.Contains(col.gameObject) && col.CompareTag("CargoBox"))
                {
                    isOccupied = true;
                    break;
                }
            }
            if (!isOccupied) return i;
        }
        return -1;
    }

    void RemoveBoxFromSpawnedList(GameObject boxInstance)
    {
        if (spawnedDeliveryBoxes.Contains(boxInstance))
        {
            spawnedDeliveryBoxes.Remove(boxInstance);
        }
    }

    public void UpdatePriceUI_BuyPanel()
    {
        if (products == null) return;
        foreach (MarketProduct product in products)
        {
            if (product != null && product.priceText != null)
            {
                product.priceText.text = $"{product.price}$";
            }
        }
    }

    public MarketProduct GetProductDefinitionByName(string name)
    {
        if (string.IsNullOrEmpty(name) || products == null) return null;
        return products.FirstOrDefault(p => p != null && p.productName == name);
    }
}
