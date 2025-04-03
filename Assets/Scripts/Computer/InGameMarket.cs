using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

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
    }

    [System.Serializable]
    public class OrderItem
    {
        public MarketProduct product;
        public int quantity;
    }

    public MarketProduct[] products;
    public Transform spawnPoint;
    public Transform[] spawnSlots;
    public PlayerBalance playerBalance;
    public SellPanel sellPanel;

    [Header("Small Cargo Boxes")]
    public GameObject smallBox1Slot;
    public GameObject smallBox2Slots;
    public GameObject smallBox3to4Slots;

    [Header("Large Cargo Boxes")]
    public GameObject largeBox1Slot;
    public GameObject largeBox2Slots;
    public GameObject largeBox3to4Slots;

    private List<GameObject> spawnedBoxes = new List<GameObject>();
    private List<OrderItem> orderBasket = new List<OrderItem>();

    void Start()
    {
        // Butonlarý baðla
        SetupBuyButtons();
        UpdatePriceUI();
    }

    public void SetupBuyButtons()
    {
        // Tüm alt GameObject’lerdeki butonlarý al
        Button[] buttons = GetComponentsInChildren<Button>(true); // true: devre dýþý olanlar da dahil
        Debug.Log($"Toplam {buttons.Length} buton bulundu.");

        for (int i = 0; i < buttons.Length; i++)
        {
            if (buttons[i].CompareTag("BuyButton"))
            {
                int index = i;
                buttons[i].onClick.RemoveAllListeners(); // Önceki listener’larý temizle
                buttons[i].onClick.AddListener(() => AddToBasket(index));
                Debug.Log($"BuyButton tag’ine sahip buton bulundu: {buttons[i].name}, index: {index}");
            }
        }

        // Eðer hiçbir BuyButton bulunamadýysa uyarý ver
        if (!System.Array.Exists(buttons, button => button.CompareTag("BuyButton")))
        {
            Debug.LogWarning("Hiçbir buton 'BuyButton' tag’ine sahip deðil! Butonlarýn tag’lerini kontrol edin.");
        }
    }

    void OnValidate()
    {
        UpdatePriceUI();
    }

    public void AddToBasket(int productIndex)
    {
        Debug.Log($"AddToBasket çaðrýldý, productIndex: {productIndex}");

        if (productIndex < 0 || productIndex >= products.Length)
        {
            Debug.LogError($"Geçersiz ürün indeksi: {productIndex}, products.Length: {products.Length}");
            return;
        }

        MarketProduct product = products[productIndex];
        Debug.Log($"Ürün: {product.productName}, Fiyat: {product.price}");

        if (playerBalance == null)
        {
            Debug.LogError("PlayerBalance referansý eksik!");
            return;
        }

        int quantityToBuy = 1;
        int totalPrice = product.price * quantityToBuy;
        Debug.Log($"Toplam fiyat: {totalPrice}, Oyuncu bakiyesi: {playerBalance.GetBalance()}");

        if (playerBalance.DeductBalance(totalPrice))
        {
            Debug.Log($"{quantityToBuy} adet {product.productName} sepete eklendi!");
            OrderItem existingOrder = orderBasket.Find(o => o.product == product);
            if (existingOrder != null)
            {
                existingOrder.quantity += quantityToBuy;
            }
            else
            {
                orderBasket.Add(new OrderItem { product = product, quantity = quantityToBuy });
            }
            product.quantity += quantityToBuy;
            if (sellPanel != null)
            {
                sellPanel.UpdateSellPanel();
            }
            else
            {
                Debug.LogWarning("SellPanel referansý eksik!");
            }
        }
        else
        {
            Debug.Log("Yetersiz bakiye!");
        }
    }

    public void ProcessOrders()
    {
        foreach (OrderItem order in orderBasket)
        {
            SpawnProductInCargoBox(order.product, order.quantity);
        }
        orderBasket.Clear();
        Debug.Log("Sipariþler iþlendi, sepet temizlendi.");
    }

    void SpawnProductInCargoBox(MarketProduct product, int quantity)
    {
        GameObject cargoBoxPrefab = SelectCargoBox(product.isLarge, quantity);
        if (cargoBoxPrefab == null)
        {
            Debug.LogError("Uygun kargo kutusu bulunamadý!");
            return;
        }

        int slotIndex = GetAvailableSlotIndex();
        if (slotIndex == -1)
        {
            Debug.LogWarning("Boþ slot yok, kutu spawn edilemedi!");
            return;
        }

        Vector3 spawnPosition = spawnSlots[slotIndex].position;
        Quaternion spawnRotation = cargoBoxPrefab.transform.rotation;
        GameObject cargoBoxInstance = Instantiate(cargoBoxPrefab, spawnPosition, spawnRotation);
        CargoBox cargoBox = cargoBoxInstance.GetComponent<CargoBox>();

        if (cargoBox == null)
        {
            Debug.LogError("Kargo kutusunda CargoBox scripti eksik!");
            Destroy(cargoBoxInstance);
            return;
        }

        Vector3 prefabScale = product.productPrefab.transform.localScale;
        Debug.Log($"{product.productName} prefab ölçeði: {prefabScale}");

        for (int i = 0; i < quantity; i++)
        {
            GameObject productInstance = Instantiate(product.productPrefab, spawnPosition, product.productPrefab.transform.rotation);
            productInstance.transform.localScale = prefabScale;
            Debug.Log($"{product.productName} spawn olduktan sonra ölçek: {productInstance.transform.localScale}");

            Product productComponent = productInstance.GetComponent<Product>();
            if (productComponent != null)
            {
                if (cargoBox.TryPlaceProduct(productComponent))
                {
                    productComponent.isHeld = false;
                    productComponent.isPlaced = true;
                }
                else
                {
                    Debug.LogError("Ürün kargo kutusuna yerleþtirilemedi!");
                    Destroy(productInstance);
                }
            }
            else
            {
                Debug.LogError($"{product.productName} prefabýnda Product scripti eksik!");
                Destroy(productInstance);
            }
        }

        spawnedBoxes.Add(cargoBoxInstance);
    }

    GameObject SelectCargoBox(bool isLarge, int quantity)
    {
        if (isLarge)
        {
            if (quantity == 1) return largeBox1Slot;
            if (quantity == 2) return largeBox2Slots;
            if (quantity >= 3 && quantity <= 4) return largeBox3to4Slots;
        }
        else
        {
            if (quantity == 1) return smallBox1Slot;
            if (quantity == 2) return smallBox2Slots;
            if (quantity >= 3 && quantity <= 4) return smallBox3to4Slots;
        }
        Debug.LogWarning("Desteklenmeyen miktar: " + quantity);
        return null;
    }

    int GetAvailableSlotIndex()
    {
        for (int i = 0; i < spawnSlots.Length; i++)
        {
            bool isOccupied = false;
            foreach (GameObject box in spawnedBoxes)
            {
                if (Vector3.Distance(box.transform.position, spawnSlots[i].position) < 0.1f)
                {
                    isOccupied = true;
                    break;
                }
            }
            if (!isOccupied) return i;
        }
        return -1;
    }

    public List<MarketProduct> GetAvailableProducts()
    {
        List<MarketProduct> availableProducts = new List<MarketProduct>();
        foreach (MarketProduct product in products)
        {
            if (product.quantity > 0)
            {
                availableProducts.Add(product);
            }
        }
        return availableProducts;
    }

    public void UpdatePriceUI()
    {
        foreach (MarketProduct product in products)
        {
            if (product.priceText != null)
            {
                string priceString = product.price.ToString() + " $";
                product.priceText.text = priceString;
                Debug.Log($"{product.productName} için fiyat güncellendi: {priceString}");
            }
            else
            {
                Debug.LogWarning($"{product.productName} için fiyat TextMeshProUGUI referansý eksik!");
            }
        }
    }
}