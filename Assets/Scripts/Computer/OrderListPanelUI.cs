using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Text;
using System.Linq;
using System.Collections.Generic; // CS0246 için eklendi

public class OrderListPanelUI : MonoBehaviour
{
    [Header("Bağlantılar")]
    [SerializeField] private CustomerOrderManager orderManager;
    [SerializeField] private GameObject siparisSatiriPrefab;
    [SerializeField] private Transform scrollviewContentParent;
    [SerializeField] private PackingStation packingStation;
    [SerializeField] private MarketScreenManager marketScreenManager;
    [SerializeField] private GameObject slipPrefab;
    [SerializeField] private Transform printerPosition;

    void Awake()
    {
        if (orderManager == null) orderManager = FindObjectOfType<CustomerOrderManager>();
        if (packingStation == null) packingStation = FindObjectOfType<PackingStation>();
        if (marketScreenManager == null) marketScreenManager = FindObjectOfType<MarketScreenManager>();
        if (slipPrefab == null) Debug.LogError("Slip Prefab atanmamış!");
        if (printerPosition == null) Debug.LogError("Printer Position atanmamış!");

        if (orderManager == null) { Debug.LogError("CustomerOrderManager referansı bulunamadı!"); enabled = false; }
        if (packingStation == null) { Debug.LogError("PackingStation referansı bulunamadı!"); }
        if (marketScreenManager == null) { Debug.LogError("MarketScreenManager referansı bulunamadı!"); }
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

    public void Show()
    {
        Debug.Log("OrderListPanelUI: Show() metodu çağrıldı.");
        gameObject.SetActive(true);
    }

    public void RefreshOrderList()
    {
        if (orderManager == null || siparisSatiriPrefab == null || scrollviewContentParent == null)
        {
            Debug.LogError("RefreshOrderList: Temel referanslardan biri eksik!");
            return;
        }

        foreach (Transform child in scrollviewContentParent)
        {
            Destroy(child.gameObject);
        }

        List<OrderData> orders = orderManager.GetPendingOrders();
        if (orders == null)
        {
            Debug.LogWarning("GetPendingOrders() null döndürdü.");
            return;
        }

        foreach (OrderData order in orders)
        {
            GameObject satirInstance = Instantiate(siparisSatiriPrefab, scrollviewContentParent);
            if (satirInstance == null)
            {
                Debug.LogError("Sipariş satırı prefab'ı Instantiate edilemedi!");
                continue;
            }

            TextMeshProUGUI siparisNoText = satirInstance.transform.Find("SiparisNo")?.GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI urunAdlariText = satirInstance.transform.Find("UrunAdlari")?.GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI siparisDurumuText = satirInstance.transform.Find("SiparisDurumu")?.GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI toplamTutarText = satirInstance.transform.Find("ToplamTutar")?.GetComponent<TextMeshProUGUI>();
            Button hazirlaButton = satirInstance.transform.Find("HazirlaButton")?.GetComponent<Button>();

            if (siparisNoText != null) siparisNoText.text = $"Sip. No: {order.orderID}";
            else Debug.LogWarning($"'{siparisSatiriPrefab.name}' prefabında 'SiparisNo' eksik!");

            if (urunAdlariText != null)
            {
                StringBuilder builder = new StringBuilder();
                if (order.itemsInOrder != null && order.itemsInOrder.Count > 0)
                {
                    for (int i = 0; i < order.itemsInOrder.Count; i++)
                    {
                        var item = order.itemsInOrder[i];
                        builder.Append($"{item.quantity} x {item.productDefinition?.productName ?? "[Bilinmeyen Ürün]"}");
                        if (i < order.itemsInOrder.Count - 1) builder.Append("\n");
                    }
                }
                else { builder.Append("Ürün Yok"); }
                urunAdlariText.text = builder.ToString();
            }
            else Debug.LogWarning($"'{siparisSatiriPrefab.name}' prefabında 'UrunAdlari' eksik!");

            if (siparisDurumuText != null) siparisDurumuText.text = $"Durum: {order.status}";
            else Debug.LogWarning($"'{siparisSatiriPrefab.name}' prefabında 'SiparisDurumu' eksik!");

            if (toplamTutarText != null) toplamTutarText.text = $"Tutar: {order.totalOrderValue}₺";
            else Debug.LogWarning($"'{siparisSatiriPrefab.name}' prefabında 'ToplamTutar' eksik!");

            if (hazirlaButton != null)
            {
                hazirlaButton.interactable = (order.status == OrderStatus.Yeni);
                hazirlaButton.onClick.RemoveAllListeners();
                OrderData currentOrder = order;
                hazirlaButton.onClick.AddListener(() => OnHazirlaButtonClicked(currentOrder));
            }
            else Debug.LogWarning($"'{siparisSatiriPrefab.name}' prefabında 'HazirlaButton' eksik!");
        }
    }

    private void OnHazirlaButtonClicked(OrderData order)
    {
        if (order == null)
        {
            Debug.LogError("OnHazirlaButtonClicked: Order verisi null!");
            return;
        }
        Debug.Log($"Hazırla butonuna tıklandı: Sipariş ID {order.orderID}, Durum: {order.status}");

        try
        {
            if (ActiveOrderManager.Instance == null)
            {
                Debug.LogError("ActiveOrderManager.Instance bulunamadı!");
                return;
            }
            ActiveOrderManager.Instance.SetActiveOrder(order);

            orderManager.UpdateOrderStatus(order.orderID, OrderStatus.Hazirlaniyor);
            PrintSlipForOrder(order); // Slip basımı
            packingStation.SpawnCargoBoxForOrder(order);
            marketScreenManager.CloseCanvas();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"OnHazirlaButtonClicked sırasında hata: {e.Message}");
        }
    }

    private void PrintSlipForOrder(OrderData order)
    {
        if (order == null)
        {
            Debug.LogWarning("PrintSlipForOrder: Order verisi null!");
            return;
        }
        if (slipPrefab == null || printerPosition == null)
        {
            Debug.LogError("Slip Prefab veya Printer Position atanmamış!");
            return;
        }

        try
        {
            GameObject slipInstance = Instantiate(slipPrefab, printerPosition.position + Vector3.up * 0.1f, Quaternion.identity);
            Slip slip = slipInstance.GetComponent<Slip>();
            if (slip != null)
            {
                slip.SetOrderData(order);
                Debug.Log($"Slip oluşturuldu: Sipariş ID {order.orderID}, Pozisyon: {slipInstance.transform.position}");
            }
            else
            {
                Debug.LogError("Slip objesinde Slip script’i eksik!");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"PrintSlipForOrder sırasında hata: {e.Message}");
        }
    }
}