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
        paymentButton.onClick.AddListener(OnPaymentClicked);
        cargoDropdown.onValueChanged.AddListener(delegate { OnCargoSelectionChanged(); });
        PopulateCargoDropdown();
        if (shoppingCartPanel != null) shoppingCartPanel.SetActive(false);
    }

    void PopulateCargoDropdown()
    {
        cargoDropdown.ClearOptions();
        List<string> options = availableCargoOptions.Select(o => $"{o.displayName} (+{o.price}$)")
                                                    .ToList();
        cargoDropdown.AddOptions(options);
        OnCargoSelectionChanged();
    }

    void OnCargoSelectionChanged()
    {
        int selectedIndex = cargoDropdown.value;
        if (selectedIndex < availableCargoOptions.Count)
        {
            selectedCargo = availableCargoOptions[selectedIndex];
            UpdateCartUI();
        }
    }

    public void UpdateCartUI()
    {
        foreach (Transform child in cartItemsParent) { Destroy(child.gameObject); }
        foreach (var item in itemsInCart)
        {
            GameObject itemUI = Instantiate(cartItemPrefab, cartItemsParent);
            itemUI.GetComponent<CartItemUI>().Setup(item);
        }
        CalculateTotal();
    }

    void CalculateTotal()
    {
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

            // --- KONTROL 1 ---
            Debug.Log("1. ÖDEME BAÞARILI! DeliveryManager'a yeni teslimat görevi veriliyor...");

            DeliveryManager.Instance.ScheduleNewDelivery(itemsInCart, selectedCargo);

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
        shoppingCartPanel.SetActive(!shoppingCartPanel.activeSelf);
        if (shoppingCartPanel.activeSelf) UpdateCartUI();
    }
}