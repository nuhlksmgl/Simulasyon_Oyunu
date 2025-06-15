using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine.UI;

public class ShoppingCart : MonoBehaviour
{
    public static ShoppingCart Instance { get; private set; }

    [System.Serializable]
    public class CartItem { public MarketProduct Product; public int Quantity; }

    public List<CartItem> itemsInCart = new List<CartItem>();

    [Header("Kargo Ayarlarý")]
    public List<CargoOption> availableCargoOptions;
    private CargoOption selectedCargo;

    [Header("Arayüz Referanslarý")]
    [SerializeField] private GameObject shoppingCartPanel;
    [SerializeField] private Transform cartItemsParent;
    [SerializeField] private GameObject cartItemPrefab;
    [SerializeField] private TextMeshProUGUI totalPriceText;
    [SerializeField] private TMP_Dropdown cargoDropdown;
    [SerializeField] private Button paymentButton;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip paymentSound;

    void Awake() { Instance = this; }

    void Start()
    {
        if (paymentButton != null) paymentButton.onClick.AddListener(OnPaymentClicked);
        if (cargoDropdown != null) cargoDropdown.onValueChanged.AddListener(delegate { OnCargoSelectionChanged(); });

        PopulateCargoDropdown();

        if (shoppingCartPanel != null) shoppingCartPanel.SetActive(false);
    }

    void PopulateCargoDropdown()
    {
        if (cargoDropdown == null || availableCargoOptions == null) return;

        cargoDropdown.ClearOptions();
        List<string> options = availableCargoOptions.Select(o => $"{o.displayName} (+{o.price}$)")
                                                    .ToList();
        cargoDropdown.AddOptions(options);
        OnCargoSelectionChanged();
    }

    void OnCargoSelectionChanged()
    {
        if (cargoDropdown == null || availableCargoOptions == null || availableCargoOptions.Count == 0) return;

        int selectedIndex = cargoDropdown.value;
        if (selectedIndex < availableCargoOptions.Count)
        {
            selectedCargo = availableCargoOptions[selectedIndex];
            UpdateCartUI();
        }
    }

    public void UpdateCartUI()
    {
        if (cartItemsParent != null)
        {
            foreach (Transform child in cartItemsParent) { Destroy(child.gameObject); }
        }

        if (cartItemPrefab != null && cartItemsParent != null)
        {
            foreach (var item in itemsInCart)
            {
                GameObject itemUI = Instantiate(cartItemPrefab, cartItemsParent);
                itemUI.GetComponent<CartItemUI>()?.Setup(item);
            }
        }

        CalculateTotal();
    }

    void CalculateTotal()
    {
        if (totalPriceText == null) return;

        float itemsTotal = itemsInCart.Sum(item => item.Product.price * item.Quantity);
        float cargoPrice = (selectedCargo != null) ? selectedCargo.price : 0;
        float finalTotal = itemsTotal + cargoPrice;

        totalPriceText.text = $"Toplam: {finalTotal:F2}$";
    }

    public void AddItem(MarketProduct product, int quantity)
    {
        if (product.isPurchased) return;
        var existingItem = itemsInCart.FirstOrDefault(item => item.Product == product);
        if (existingItem != null) { existingItem.Quantity += quantity; }
        else { itemsInCart.Add(new CartItem { Product = product, Quantity = quantity }); }
        UpdateCartUI();
    }

    public void RemoveItem(CartItem itemToRemove)
    {
        if (itemsInCart.Contains(itemToRemove))
        {
            itemsInCart.Remove(itemToRemove);
            UpdateCartUI();
        }
    }

    public void OnPaymentClicked()
    {
        float totalCost = itemsInCart.Sum(item => item.Product.price * item.Quantity)
                        + ((selectedCargo != null) ? selectedCargo.price : 0);

        if (PlayerBalance.Instance.DeductBalance(totalCost))
        {
            if (audioSource != null && paymentSound != null) audioSource.PlayOneShot(paymentSound);

            List<CartItem> itemsForBoxedDelivery = new List<CartItem>();

            foreach (var item in itemsInCart)
            {
                if (item.Product.baseMarketPrice <= 0)
                {
                    item.Product.baseMarketPrice = item.Product.price;
                    item.Product.currentAverageMarketPrice = item.Product.baseMarketPrice * Random.Range(1.05f, 1.15f);
                }

                if (!item.Product.isInstantPurchase)
                {
                    item.Product.inTransitStock += item.Quantity;
                }

                if (item.Product.isInstantPurchase)
                {
                    for (int i = 0; i < item.Quantity; i++) { InGameMarket.Instance.ApplyInstantPurchase(item.Product); }
                }
                else if (item.Product.isDirectDelivery)
                {
                    for (int i = 0; i < item.Quantity; i++) { DeliveryManager.Instance.ScheduleDirectDelivery(item.Product, selectedCargo); }
                }
                else
                {
                    itemsForBoxedDelivery.Add(item);
                }
            }

            if (itemsForBoxedDelivery.Count > 0)
            {
                DeliveryManager.Instance.ScheduleNewDelivery(itemsForBoxedDelivery, selectedCargo);
            }

            itemsInCart.Clear();
            UpdateCartUI();
        }
        else
        {
            Debug.Log("Yetersiz Bakiye!");
        }
    }

    public void ToggleCartPanel()
    {
        if (shoppingCartPanel == null) return;
        shoppingCartPanel.SetActive(!shoppingCartPanel.activeSelf);
        if (shoppingCartPanel.activeSelf) UpdateCartUI();
    }
}