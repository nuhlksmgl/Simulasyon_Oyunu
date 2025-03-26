using UnityEngine;
using UnityEngine.UI;
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
        Button[] buttons = GetComponentsInChildren<Button>();
        for (int i = 0; i < buttons.Length; i++)
        {
            int index = i;
            if (buttons[i].CompareTag("BuyButton"))
            {
                buttons[i].onClick.AddListener(() => AddToBasket(index));
            }
        }
    }

    public void AddToBasket(int productIndex)
    {
        if (productIndex < 0 || productIndex >= products.Length)
        {
            Debug.LogError("Geçersiz ürün indeksi!");
            return;
        }

        MarketProduct product = products[productIndex];
        int quantityToBuy = 1;
        int totalPrice = product.price * quantityToBuy;

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
            sellPanel.UpdateSellPanel();
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

        Vector3 prefabScale = product.productPrefab.transform.localScale; // Prefabýn orijinal ölçeði
        Debug.Log($"{product.productName} prefab ölçeði: {prefabScale}");

        for (int i = 0; i < quantity; i++)
        {
            GameObject productInstance = Instantiate(product.productPrefab, spawnPosition, product.productPrefab.transform.rotation);
            productInstance.transform.localScale = prefabScale; // Prefabýn orijinal ölçeðini uygula
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
}