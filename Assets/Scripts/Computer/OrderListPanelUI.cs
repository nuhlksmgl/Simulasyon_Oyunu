using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Text; // StringBuilder için
using System.Linq; // FirstOrDefault gibi kullanımlar için (ileride gerekirse)

public class OrderListPanelUI : MonoBehaviour
{
    [Header("Bağlantılar")]
    public CustomerOrderManager customerOrderManager;
    public GameObject siparisSatiriPrefab; // Inspector'dan atanacak UI prefab'ı
    public Transform scrollviewContentParent; // ScrollView'in Content objesi
    public PackingStation packingStation;
    public MarketScreenManager marketScreenManager; // Paneli kapatmak için

    void Awake()
    {
        // Referansları Awake'te bulmak veya kontrol etmek daha güvenli olabilir
        // Eğer Inspector'dan atanacaksa bu FindObjectOfType çağrılarına gerek kalmayabilir.
        // Ancak atanmamışsa diye bir güvenlik önlemi olarak eklenebilir.
        if (customerOrderManager == null)
            customerOrderManager = FindObjectOfType<CustomerOrderManager>();
        if (packingStation == null)
            packingStation = FindObjectOfType<PackingStation>();
        if (marketScreenManager == null)
            marketScreenManager = FindObjectOfType<MarketScreenManager>();

        if (customerOrderManager == null)
            Debug.LogError("OrderListPanelUI: Awake - CustomerOrderManager referansı bulunamadı veya atanmamış!");
        if (packingStation == null)
            Debug.LogError("OrderListPanelUI: Awake - PackingStation referansı bulunamadı veya atanmamış!");
        if (marketScreenManager == null)
            Debug.LogError("OrderListPanelUI: Awake - MarketScreenManager referansı bulunamadı veya atanmamış!");
    }

    void OnEnable()
    {
        if (customerOrderManager == null)
        {
            Debug.LogError("OrderListPanelUI: OnEnable - CustomerOrderManager referansı null! Panel düzgün çalışmayabilir. Lütfen Inspector'dan atayın.");
            gameObject.SetActive(false); // Kendini devre dışı bırak
            return;
        }
        // Statik event'e sınıf adı üzerinden abone ol
        CustomerOrderManager.OnOrderListChanged += HandleOrderListChanged;
        Debug.Log("OrderListPanelUI OnEnable: CustomerOrderManager.OnOrderListChanged event'ine abone olundu.");
        RefreshOrderList(); // Panel açıldığında listeyi hemen güncelle
    }

    void OnDisable()
    {
        // Statik event'ten sınıf adı üzerinden abonelikten çık
        // CustomerOrderManager objesi yok edilmiş olsa bile static event'e erişmeye çalışmak sorun yaratmaz,
        // ama event'in kendisi null olabilir (hiç abone olmadıysa veya tüm aboneler çıktıysa).
        // Genellikle bu kontrol gereksizdir ama ekstra güvenlik için yapılabilir.
        // En önemlisi, CustomerOrderManager.Instance gibi bir şeye ihtiyaç olmaması.
        CustomerOrderManager.OnOrderListChanged -= HandleOrderListChanged;
        Debug.Log("OrderListPanelUI OnDisable: CustomerOrderManager.OnOrderListChanged event aboneliği kaldırıldı.");
    }

    private void HandleOrderListChanged()
    {
        Debug.Log("OrderListPanelUI: HandleOrderListChanged çağrıldı (CustomerOrderManager.OnOrderListChanged event'i ile). Liste yenileniyor.");
        RefreshOrderList();
    }

    // Bu metod MarketScreenManager tarafından çağrılarak paneli görünür yapar.
    public void Show()
    {
        Debug.Log("OrderListPanelUI: Show() metodu çağrıldı.");
        gameObject.SetActive(true);
        // OnEnable metodu zaten RefreshOrderList'i çağıracağı için burada tekrar çağırmak genellikle gereksizdir.
        // Ancak panel zaten aktifken Show çağrılırsa OnEnable tetiklenmeyeceği için
        // bir RefreshOrderList() çağrısı burada da mantıklı olabilir.
        // RefreshOrderList(); // Eğer OnEnable dışında da anlık güncelleme isteniyorsa.
    }

    public void RefreshOrderList()
    {
        if (customerOrderManager == null || siparisSatiriPrefab == null || scrollviewContentParent == null)
        {
            Debug.LogError("OrderListPanelUI - RefreshOrderList: Temel referanslardan biri (CustomerOrderManager, SiparisSatiriPrefab, ScrollviewContentParent) atanmamış! Lütfen Inspector'dan kontrol edin.");
            return;
        }

        // Debug.Log("OrderListPanelUI: RefreshOrderList başlıyor...");

        // Önce mevcut tüm sipariş satırlarını temizle
        foreach (Transform child in scrollviewContentParent)
        {
            Destroy(child.gameObject);
        }

        List<OrderData> orders = customerOrderManager.GetPendingOrders();
        if (orders == null)
        {
            Debug.LogWarning("OrderListPanelUI: GetPendingOrders() null bir liste döndürdü.");
            return;
        }

        // Debug.Log($"OrderListPanelUI: Gösterilecek sipariş sayısı: {orders.Count}");

        if (orders.Count == 0)
        {
            // Opsiyonel: "Gösterilecek sipariş yok" mesajı için bir UI elemanı aktif edilebilir.
            // Debug.Log("OrderListPanelUI: Gösterilecek bekleyen sipariş bulunmuyor.");
        }

        foreach (OrderData order in orders)
        {
            GameObject satirInstance = Instantiate(siparisSatiriPrefab, scrollviewContentParent);
            if (satirInstance == null)
            {
                Debug.LogError("Sipariş satırı prefab'ı Instantiate edilemedi! Prefab doğru atanmış mı?");
                continue;
            }

            // --- PREFAB İÇİNDEKİ UI ELEMANLARININ İSİMLERİNİN DOĞRULUĞUNU KONTROL ET ---
            // İsimler prefab'ınızdaki TextMeshPro ve Button objelerinin isimleriyle BİREBİR AYNI OLMALIDIR.
            TextMeshProUGUI siparisNoText = satirInstance.transform.Find("SiparisNo")?.GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI urunAdlariText = satirInstance.transform.Find("UrunAdlari")?.GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI siparisDurumuText = satirInstance.transform.Find("SiparisDurumu")?.GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI toplamTutarText = satirInstance.transform.Find("ToplamTutar")?.GetComponent<TextMeshProUGUI>();
            Button hazirlaButton = satirInstance.transform.Find("HazirlaButton")?.GetComponent<Button>();
            // --- PREFAB İÇİNDEKİ İSİMLERİN DOĞRULUĞUNU KONTROL ET ---

            if (siparisNoText != null) siparisNoText.text = $"Sip. No: {order.orderID}";
            else Debug.LogWarning($"'{siparisSatiriPrefab.name}' prefabı içinde 'SiparisNo_Text' isimli TextMeshPro objesi bulunamadı veya TextMeshPro component'i eksik!");

            if (urunAdlariText != null)
            {
                StringBuilder builder = new StringBuilder();
                if (order.itemsInOrder != null && order.itemsInOrder.Count > 0)
                {
                    for (int i = 0; i < order.itemsInOrder.Count; i++)
                    {
                        var item = order.itemsInOrder[i];
                        if (item.productDefinition != null)
                        {
                            builder.Append($"{item.quantity} x {item.productDefinition.productName}");
                        }
                        else { builder.Append($"{item.quantity} x [Bilinmeyen Ürün]"); }

                        if (i < order.itemsInOrder.Count - 1) builder.Append("\n"); // Ürünleri alt alta listele
                    }
                }
                else { builder.Append("Ürün Yok"); }
                urunAdlariText.text = builder.ToString();
            }
            else Debug.LogWarning($"'{siparisSatiriPrefab.name}' prefabı içinde 'UrunAdlari_Text' isimli TextMeshPro objesi bulunamadı veya TextMeshPro component'i eksik!");

            if (siparisDurumuText != null) siparisDurumuText.text = $"Durum: {order.status.ToString()}";
            else Debug.LogWarning($"'{siparisSatiriPrefab.name}' prefabı içinde 'SiparisDurumu_Text' isimli TextMeshPro objesi bulunamadı veya TextMeshPro component'i eksik!");

            if (toplamTutarText != null) toplamTutarText.text = $"Tutar: {order.totalOrderValue}₺";
            else Debug.LogWarning($"'{siparisSatiriPrefab.name}' prefabı içinde 'ToplamTutar_Text' isimli TextMeshPro objesi bulunamadı veya TextMeshPro component'i eksik!");

            if (hazirlaButton != null)
            {
                hazirlaButton.interactable = (order.status == OrderStatus.Yeni); // Sadece "Yeni" siparişler hazırlanabilir
                hazirlaButton.onClick.RemoveAllListeners(); // Önceki listener'ları temizle
                OrderData currentOrderForButton = order; // Lambda için order'ı yakala
                hazirlaButton.onClick.AddListener(() => OnHazirlaButtonClicked(currentOrderForButton));
            }
            else Debug.LogWarning($"'{siparisSatiriPrefab.name}' prefabı içinde 'HazirlaButton' isimli Button objesi bulunamadı veya Button component'i eksik!");
        }
        // Debug.Log("OrderListPanelUI: RefreshOrderList tamamlandı.");
    }

    void OnHazirlaButtonClicked(OrderData order)
    {
        if (order == null)
        {
            Debug.LogError("OnHazirlaButtonClicked: Order verisi null!");
            return;
        }
        Debug.Log($"Hazırla butonuna tıklandı: Sipariş ID {order.orderID}, Durum: {order.status}");

        if (ActiveOrderManager.Instance == null)
        {
            Debug.LogError("ActiveOrderManager.Instance bulunamadı! Lütfen sahnede bir ActiveOrderManager olduğundan ve Instance'ının doğru set edildiğinden emin olun.");
            return;
        }
        ActiveOrderManager.Instance.SetActiveOrder(order);

        if (customerOrderManager == null)
        {
            Debug.LogError("OnHazirlaButtonClicked: CustomerOrderManager referansı null!");
            return;
        }
        customerOrderManager.UpdateOrderStatus(order.orderID, OrderStatus.Hazirlaniyor);
        // UpdateOrderStatus zaten OnOrderListChanged event'ini tetikleyeceği için liste güncellenecektir.

        if (packingStation == null)
        {
            Debug.LogError("PackingStation referansı OrderListPanelUI'da atanmamış!");
            return;
        }
        packingStation.SpawnCargoBoxForOrder(order); // Bu sipariş için boş kutu spawn et

        if (marketScreenManager == null)
        {
            Debug.LogWarning("MarketScreenManager referansı atanmamış, bilgisayar canvas'ı kapatılamadı.");
            return;
        }
        marketScreenManager.CloseCanvas(); // Bilgisayar arayüzünü kapat, oyuncu paketlemeye gitsin
    }
}
