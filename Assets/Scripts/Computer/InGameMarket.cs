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
        public int price;
        public GameObject productPrefab;
        public int quantity = 0;
        public bool isLarge;
        public TextMeshProUGUI priceText;
        public float baseMarketPrice;
        public float currentAverageMarketPrice;
        [HideInInspector]
        public int priceTrendStreak = 0;
    }

    public MarketProduct[] products;
    public Transform[] spawnSlots;
    public PlayerBalance playerBalance;
    public SellPanel sellPanel;

    [Header("Small Cargo Boxes Prefabs")]
    public GameObject smallBox1SlotPrefab;
    public GameObject smallBox2SlotsPrefab;
    public GameObject smallBox3to4SlotsPrefab;

    [Header("Large Cargo Boxes Prefabs")]
    public GameObject largeBox1SlotPrefab;
    public GameObject largeBox2SlotsPrefab;
    public GameObject largeBox3to4SlotsPrefab;

    private List<GameObject> spawnedDeliveryBoxes = new List<GameObject>();
    private List<OrderItemDetail> purchaseBasket = new List<OrderItemDetail>();

    void Start()
    {
        InitializeAllProductMarketPrices();
        SetupBuyButtons();
        UpdatePriceUI_BuyPanel();
    }

    public void InitializeAllProductMarketPrices()
    {
        if (products == null) return;
        foreach (MarketProduct product in products)
        {
            if (product == null) continue;
            if (product.baseMarketPrice <= 0)
                product.baseMarketPrice = product.price > 0 ? Mathf.Round(product.price * 1.5f) : 50f;
            if (product.currentAverageMarketPrice <= 0)
                product.currentAverageMarketPrice = product.baseMarketPrice;
            product.priceTrendStreak = 0;
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

    public void AddToPurchaseBasket(int productIndex)
    {
        if (productIndex < 0 || productIndex >= products.Length || playerBalance == null) return;

        MarketProduct selectedProduct = products[productIndex];
        int costOfPurchase = selectedProduct.price;

        if (playerBalance.DeductBalance(costOfPurchase))
        {
            OrderItemDetail existingItem = purchaseBasket.Find(item => item.productDefinition == selectedProduct);
            if (existingItem != null)
                existingItem.quantity++;
            else
                purchaseBasket.Add(new OrderItemDetail(selectedProduct, 1, selectedProduct.price, (int)selectedProduct.currentAverageMarketPrice));

            selectedProduct.quantity++;
            sellPanel?.UpdateSellPanel();
        }
    }

    public void ProcessPurchaseBasket()
    {
        if (purchaseBasket.Count == 0) return;
        foreach (OrderItemDetail itemDetail in purchaseBasket)
        {
            SpawnProductsInDeliveryBox(itemDetail.productDefinition, itemDetail.quantity);
        }
        purchaseBasket.Clear();
        sellPanel?.UpdateSellPanel();
    }

    void SpawnProductsInDeliveryBox(MarketProduct product, int quantity)
    {
        if (product == null || product.productPrefab == null) return;
        GameObject boxPrefabToSpawn = SelectCargoBoxPrefab(product.isLarge, quantity);
        if (boxPrefabToSpawn == null) return;
        int slotIndex = GetAvailableDeliverySlotIndex();
        if (slotIndex == -1) return;

        Transform selectedSpawnSlot = spawnSlots[slotIndex];
        GameObject boxInstance = Instantiate(boxPrefabToSpawn, selectedSpawnSlot.position, Quaternion.identity);
        spawnedDeliveryBoxes.Add(boxInstance);

        CargoBoxProxy proxy = boxInstance.GetComponent<CargoBoxProxy>();
        if (proxy == null || proxy.RealCargoBox == null)
        {
            Debug.LogError($"Prefabde Proxy/RealCargoBox eksik: {boxPrefabToSpawn.name}");
            Destroy(boxInstance);
            return;
        }

        CargoBox cargoBoxScript = proxy.RealCargoBox;
        OrderData supplierDeliveryOrder = new OrderData();
        supplierDeliveryOrder.InitializeCustomer();
        supplierDeliveryOrder.customerName = $"Tedarikçi - {product.productName}";
        cargoBoxScript.AssignOrder(supplierDeliveryOrder);

        cargoBoxScript.SetLidStateForced(true);

        for (int i = 0; i < quantity; i++)
        {
            GameObject productObjInstance = Instantiate(product.productPrefab);
            Product productScriptComponent = productObjInstance.GetComponent<Product>();
            if (productScriptComponent != null)
            {
                productScriptComponent.productDefinition = product;
                if (!cargoBoxScript.TryPlaceProduct(productScriptComponent))
                {
                    Debug.LogError($"Ürün yerleştirilemedi: {product.productName}");
                    Destroy(productObjInstance);
                }
            }
            else
            {
                Debug.LogError($"Ürün prefabında Product scripti yok: {product.productPrefab.name}");
                Destroy(productObjInstance);
            }
        }
        cargoBoxScript.SetLidStateForced(false);
    }

    public GameObject SpawnEmptyOrderBoxForCustomer(OrderData customerOrder, Transform spawnTransform)
    {
        if (customerOrder == null || customerOrder.itemsInOrder.Count == 0) return null;
        int totalQuantity = customerOrder.itemsInOrder.Sum(item => item.quantity);
        bool containsLarge = customerOrder.itemsInOrder.Any(item => item.productDefinition.isLarge);
        GameObject boxPrefabToSpawn = SelectCargoBoxPrefab(containsLarge, totalQuantity);
        if (boxPrefabToSpawn == null) return null;

        GameObject boxInstance = Instantiate(boxPrefabToSpawn, spawnTransform.position, Quaternion.identity);

        CargoBoxProxy proxy = boxInstance.GetComponent<CargoBoxProxy>();
        if (proxy != null && proxy.RealCargoBox != null)
        {
            CargoBox cargoBoxScript = proxy.RealCargoBox;
            cargoBoxScript.AssignOrder(customerOrder);
            return boxInstance;
        }
        else
        {
            Debug.LogError($"Kargo kutusu prefabı ({boxPrefabToSpawn.name}) üzerinde CargoBoxProxy scripti veya referansı eksik!");
            Destroy(boxInstance);
            return null;
        }
    }

    GameObject SelectCargoBoxPrefab(bool isLarge, int quantity)
    {
        if (isLarge)
        {
            if (quantity == 1) return largeBox1SlotPrefab;
            if (quantity == 2) return largeBox2SlotsPrefab;
            if (quantity >= 3 && quantity <= 4) return largeBox3to4SlotsPrefab;
        }
        else
        {
            if (quantity == 1) return smallBox1SlotPrefab;
            if (quantity == 2) return smallBox2SlotsPrefab;
            if (quantity >= 3 && quantity <= 4) return smallBox3to4SlotsPrefab;
        }
        Debug.LogWarning($"Uygun kargo kutusu prefabı bulunamadı. Büyük mü: {isLarge}, Adet: {quantity}.");
        return null;
    }

    int GetAvailableDeliverySlotIndex()
    {
        for (int i = 0; i < spawnSlots.Length; i++)
        {
            if (spawnSlots[i] == null) continue;
            bool isOccupied = Physics.CheckSphere(spawnSlots[i].position, 0.2f, LayerMask.GetMask("CargoBox"));
            if (!isOccupied) return i;
        }
        return -1;
    }

    // Düzeltilmiş Fonksiyon
    public MarketProduct GetProductDefinitionByName(string name)
    {
        if (products == null || string.IsNullOrEmpty(name))
        {
            return null;
        }
        // FirstOrDefault, koşulu sağlayan ilk elemanı veya bulamazsa varsayılan değeri (class'lar için null) döndürür.
        // Bu, tüm kod yollarının bir değer döndürmesini sağlar.
        return products.FirstOrDefault(p => p != null && p.productName == name);
    }

    // Diğer yardımcı fonksiyonlar
    void RemoveBoxFromSpawnedList(GameObject boxInstance) { spawnedDeliveryBoxes.Remove(boxInstance); }
    public void UpdatePriceUI_BuyPanel() { /*...*/ }
}