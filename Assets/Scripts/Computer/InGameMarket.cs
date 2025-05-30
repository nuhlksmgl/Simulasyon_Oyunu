
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
        public GameObject prefab;
        public int quantity = 0;
        public bool isLarge;
        public TextMeshProUGUI priceText;
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
    private List<OrderItemDetail> orderBasket = new List<OrderItemDetail>();

    void Start()
    {
        SetupBuyButtons();
        UpdatePriceUI();
    }

    public void SetupBuyButtons()
    {
        Button[] buttons = GetComponentsInChildren<Button>(true);
        for (int i = 0; i < buttons.Length; i++)
        {
            if (buttons[i].CompareTag("BuyButton"))
            {
                int index = i;
                buttons[i].onClick.RemoveAllListeners();
                buttons[i].onClick.AddListener(() => AddToBasket(index));
            }
        }
    }

    void OnValidate()
    {
        UpdatePriceUI();
    }

    public void AddToBasket(int productIndex)
    {
        if (productIndex < 0 || productIndex >= products.Length) return;

        MarketProduct product = products[productIndex];
        if (playerBalance == null) return;

        int quantityToBuy = 1;
        int totalPrice = product.price * quantityToBuy;

        if (playerBalance.DeductBalance(totalPrice))
        {
            OrderItemDetail existingOrder = orderBasket.Find(o => o.productDefinition == product);
            if (existingOrder != null)
            {
                existingOrder.quantity += quantityToBuy;
            }
            else
            {
                orderBasket.Add(new OrderItemDetail(product, quantityToBuy, product.price));
            }

            product.quantity += quantityToBuy;

            if (sellPanel != null)
            {
                sellPanel.UpdateSellPanel();
            }
        }
    }

    public void ProcessOrders()
    {
        foreach (OrderItemDetail order in orderBasket)
        {
            SpawnPurchaseBox(order.productDefinition, order.quantity);
        }

        orderBasket.Clear();

        if (sellPanel != null)
            sellPanel.UpdateSellPanel();
    }

    public void SpawnPurchaseBox(MarketProduct product, int quantity)
    {
        GameObject boxPrefab = SelectCargoBox(product.isLarge, quantity);
        if (boxPrefab == null) return;

        int slotIndex = GetAvailableSlotIndex();
        if (slotIndex == -1) return;

        Vector3 spawnPosition = spawnSlots[slotIndex].position;
        Quaternion rotation = boxPrefab.transform.rotation;

        GameObject boxInstance = Instantiate(boxPrefab, spawnPosition, rotation);
        CargoBox cargoBox = boxInstance.GetComponent<CargoBox>();
        if (cargoBox == null) return;

        OrderData fakeOrder = new OrderData();
        fakeOrder.totalOrderValue = product.price * quantity;
        fakeOrder.itemsInOrder.Add(new OrderItemDetail(product, quantity, product.price));
        cargoBox.AssignOrder(fakeOrder);

        for (int i = 0; i < quantity; i++)
        {
            GameObject productObj = Instantiate(product.prefab, spawnPosition, product.prefab.transform.rotation);
            productObj.transform.localScale = product.prefab.transform.localScale;

            Product prod = productObj.GetComponent<Product>();
            if (prod != null)
            {
                prod.productDefinition = product;
                bool placed = cargoBox.TryPlaceProduct(prod);
                Debug.Log(placed ? $"✔️ {product.productName} kutuya yerleştirildi." : $"❌ {product.productName} yerleştirilemedi.");
            }
            else
            {
                Debug.LogError($"❌ Prefab {product.productName} içinde Product script yok!");
                Destroy(productObj);
            }
        }

        spawnedBoxes.Add(boxInstance);
    }

    public void SpawnOrderBox(OrderData order)
    {
        int totalQty = 0;
        bool isLarge = false;

        foreach (var item in order.itemsInOrder)
        {
            totalQty += item.quantity;
            if (item.productDefinition.isLarge)
                isLarge = true;
        }

        GameObject boxPrefab = SelectCargoBox(isLarge, totalQty);
        if (boxPrefab == null) return;

        int slotIndex = GetAvailableSlotIndex();
        if (slotIndex == -1) return;

        Vector3 spawnPosition = spawnSlots[slotIndex].position;
        Quaternion rotation = boxPrefab.transform.rotation;

        GameObject boxInstance = Instantiate(boxPrefab, spawnPosition, rotation);
        CargoBox cargoBox = boxInstance.GetComponent<CargoBox>();
        if (cargoBox != null)
        {
            cargoBox.AssignOrder(order);
            spawnedBoxes.Add(boxInstance);
        }
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

    public void UpdatePriceUI()
    {
        foreach (MarketProduct product in products)
        {
            if (product.priceText != null)
            {
                string priceString = product.price.ToString() + " $";
                product.priceText.text = priceString;
            }
        }
    }

    public MarketProduct GetDefinitionForInstance(GameObject instance)
    {
        foreach (var product in products)
        {
            if (product.prefab.name == instance.name.Replace("(Clone)", "").Trim())
            {
                return product;
            }
        }

        Debug.LogWarning($"❌ Tanım bulunamadı: {instance.name}");
        return null;
    }
}
