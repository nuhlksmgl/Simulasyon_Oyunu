using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Text;
using System.Collections.Generic;

public class OrderListPanelUI : MonoBehaviour
{
    // --- DEĞİŞİKLİK: DEĞİŞKEN İSİMLERİ VE GRUPLANDIRMA GÜNCELLENDİ ---

    [Header("Yöneticiler ve Kontrolcüler")]
    [SerializeField] private CustomerOrderManager customerOrderManager;
    [SerializeField] private MarketScreenManager marketScreenManager;
    [SerializeField] private ActiveOrderManager activeOrderManager;

    [Header("Arayüz Prefabları")]
    [SerializeField] private GameObject orderRowPrefab;       // siparisSatiriPrefab -> orderRowPrefab
    [SerializeField] private GameObject orderItemIconPrefab;
    [SerializeField] private GameObject slipPrefab;

    [Header("Sahne Referansları")]
    [SerializeField] private Transform orderListContent;     // scrollviewContentParent -> orderListContent
    [SerializeField] private Transform slipSpawnPoint;       // printerPosition -> slipSpawnPoint

    void Awake()
    {
        // Referansları bulma (eğer Inspector'dan atanmamışsa)
        if (customerOrderManager == null) customerOrderManager = FindObjectOfType<CustomerOrderManager>();
        if (marketScreenManager == null) marketScreenManager = FindObjectOfType<MarketScreenManager>();
        if (activeOrderManager == null) activeOrderManager = FindObjectOfType<ActiveOrderManager>();

        // Hata kontrolleri
        if (customerOrderManager == null) Debug.LogError("CustomerOrderManager referansı bulunamadı!");
        if (marketScreenManager == null) Debug.LogError("MarketScreenManager referansı bulunamadı!");
        if (activeOrderManager == null) Debug.LogError("ActiveOrderManager referansı bulunamadı!");
    }

    void OnEnable()
    {
        CustomerOrderManager.OnOrderListChanged += RefreshOrderList;
        RefreshOrderList();
    }

    void OnDisable()
    {
        CustomerOrderManager.OnOrderListChanged -= HandleOrderListChanged;
    }

    private void HandleOrderListChanged()
    {
        RefreshOrderList();
    }

    public void RefreshOrderList()
    {
        if (customerOrderManager == null || orderRowPrefab == null || orderListContent == null) return;

        foreach (Transform child in orderListContent)
        {
            Destroy(child.gameObject);
        }

        List<OrderData> orders = customerOrderManager.GetPendingOrders();
        if (orders == null) return;

        foreach (OrderData order in orders)
        {
            GameObject satirInstance = Instantiate(orderRowPrefab, orderListContent);

            var siparisNoText = satirInstance.transform.Find("SiparisNoText")?.GetComponent<TextMeshProUGUI>();
            var kargoTuruText = satirInstance.transform.Find("KargoTuruText")?.GetComponent<TextMeshProUGUI>();
            var toplamTutarText = satirInstance.transform.Find("ToplamTutarText")?.GetComponent<TextMeshProUGUI>();
            var urunlerLayout = satirInstance.transform.Find("UrunlerLayout");
            var hazirlaButton = satirInstance.transform.Find("HazirlaButton")?.GetComponent<Button>();

            if (siparisNoText != null) siparisNoText.text = $"Sip. No: {order.orderID}";
            if (kargoTuruText != null) kargoTuruText.text = $"Kargo Türü: {order.orderType}";
            if (toplamTutarText != null) toplamTutarText.text = $"Toplam Tutar: {order.totalOrderValue:F2}₺";

            if (urunlerLayout != null && orderItemIconPrefab != null)
            {
                foreach (var item in order.itemsInOrder)
                {
                    GameObject iconInstance = Instantiate(orderItemIconPrefab, urunlerLayout);
                    var iconScript = iconInstance.GetComponent<OrderItemIconUI>();
                    if (iconScript != null) iconScript.Setup(item);
                }
            }

            if (hazirlaButton != null)
            {
                hazirlaButton.interactable = (order.status == OrderStatus.Yeni);
                hazirlaButton.onClick.RemoveAllListeners();
                hazirlaButton.onClick.AddListener(() => OnHazirlaButtonClicked(order));
            }
        }
    }

    private void OnHazirlaButtonClicked(OrderData order)
    {
        if (order == null) return;

        activeOrderManager.SetActiveOrder(order);
        customerOrderManager.UpdateOrderStatus(order.orderID, OrderStatus.Hazirlaniyor);
        PrintSlipForOrder(order);
        marketScreenManager.CloseCanvas();
    }

    private void PrintSlipForOrder(OrderData order)
    {
        if (slipPrefab == null || slipSpawnPoint == null) return;

        GameObject slipInstance = Instantiate(slipPrefab, slipSpawnPoint.position, slipSpawnPoint.rotation);
        Slip slipScript = slipInstance.GetComponent<Slip>();
        if (slipScript != null)
        {
            slipScript.SetOrderData(order);
        }
    }
}