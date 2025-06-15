// OrderListPanelUI.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Text;
using System.Collections.Generic;

public class OrderListPanelUI : MonoBehaviour
{
    [Header("Bağlantılar")]
    [SerializeField] private CustomerOrderManager orderManager;
    [SerializeField] private GameObject siparisSatiriPrefab;
    [SerializeField] private Transform scrollviewContentParent;
    [SerializeField] private PackingStation packingStation; // Bu script'in de projede olması gerekir
    [SerializeField] private MarketScreenManager marketScreenManager;
    [SerializeField] private GameObject slipPrefab;
    [SerializeField] private Transform printerPosition;

    void Awake()
    {
        // Referansları bulma
        if (orderManager == null) orderManager = FindObjectOfType<CustomerOrderManager>();
        if (packingStation == null) packingStation = FindObjectOfType<PackingStation>();
        if (marketScreenManager == null) marketScreenManager = FindObjectOfType<MarketScreenManager>();
        // Hata kontrolleri
        if (orderManager == null) Debug.LogError("CustomerOrderManager referansı bulunamadı!");
        if (packingStation == null) Debug.LogWarning("PackingStation referansı bulunamadı!");
        if (marketScreenManager == null) Debug.LogError("MarketScreenManager referansı bulunamadı!");
        if (slipPrefab == null) Debug.LogError("Slip Prefab atanmamış!");
        if (printerPosition == null) Debug.LogError("Printer Position atanmamış!");
    }

    void OnEnable()
    {
        CustomerOrderManager.OnOrderListChanged += HandleOrderListChanged;
        Debug.Log("OrderListPanelUI OnEnable: OnOrderListChanged event'ine abone olundu.");
        RefreshOrderList();
    }

    void OnDisable()
    {
        CustomerOrderManager.OnOrderListChanged -= HandleOrderListChanged;
        Debug.Log("OrderListPanelUI OnDisable: OnOrderListChanged event aboneliği kaldırıldı.");
    }

    private void HandleOrderListChanged()
    {
        Debug.Log("HandleOrderListChanged çağrıldı. Liste yenileniyor.");
        RefreshOrderList();
    }

    public void RefreshOrderList()
    {
        if (orderManager == null || siparisSatiriPrefab == null || scrollviewContentParent == null) return;

        foreach (Transform child in scrollviewContentParent)
        {
            Destroy(child.gameObject);
        }

        List<OrderData> orders = orderManager.GetPendingOrders();
        if (orders == null) return;

        foreach (OrderData order in orders)
        {
            GameObject satirInstance = Instantiate(siparisSatiriPrefab, scrollviewContentParent);

            TextMeshProUGUI siparisNoText = satirInstance.transform.Find("SiparisNo")?.GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI urunAdlariText = satirInstance.transform.Find("UrunAdlari")?.GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI siparisDurumuText = satirInstance.transform.Find("SiparisDurumu")?.GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI toplamTutarText = satirInstance.transform.Find("ToplamTutar")?.GetComponent<TextMeshProUGUI>();
            Button hazirlaButton = satirInstance.transform.Find("HazirlaButton")?.GetComponent<Button>();

            if (siparisNoText != null) siparisNoText.text = $"Sip. No: {order.orderID}";

            if (urunAdlariText != null)
            {
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < order.itemsInOrder.Count; i++)
                {
                    var item = order.itemsInOrder[i];
                    builder.Append($"{item.quantity} x {item.productDefinition?.productName ?? "[Silinmiş Ürün]"}");
                    if (i < order.itemsInOrder.Count - 1) builder.Append("\n");
                }
                urunAdlariText.text = builder.ToString();
            }

            if (siparisDurumuText != null) siparisDurumuText.text = $"Durum: {order.status}";
            if (toplamTutarText != null) toplamTutarText.text = $"Tutar: {order.totalOrderValue:F2}₺";

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
        Debug.Log($"Hazırla butonuna tıklandı: Sipariş ID {order.orderID}");

        // ActiveOrderManager gibi bir singleton varsa kullanılabilir.
        // ActiveOrderManager.Instance.SetActiveOrder(order);

        orderManager.UpdateOrderStatus(order.orderID, OrderStatus.Hazirlaniyor);

        if (packingStation != null) packingStation.SpawnCargoBoxForOrder(order);
        else Debug.LogWarning("Packing station bulunamadığı için kargo kutusu oluşturulamadı.");

        PrintSlipForOrder(order);
        marketScreenManager.CloseCanvas();
    }

    private void PrintSlipForOrder(OrderData order)
    {
        if (order == null || slipPrefab == null || printerPosition == null) return;

        GameObject slipInstance = Instantiate(slipPrefab, printerPosition.position, printerPosition.rotation);
        // Slip script'inin SetOrderData gibi bir metodu olduğunu varsayıyoruz.
        // slipInstance.GetComponent<Slip>()?.SetOrderData(order); 
        Debug.Log($"Slip oluşturuldu: Sipariş ID {order.orderID}");
    }
}