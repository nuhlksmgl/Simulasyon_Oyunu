using UnityEngine;
using TMPro;
using System.Text;

public class Slip : MonoBehaviour
{
    private OrderData orderData;
    [SerializeField] private Renderer slipRenderer;
    [SerializeField] private TextMeshProUGUI textMeshPro;
    private Color originalColor;
    private bool isHeld; // Bu değişkenin yönetimi ObjectPickup'ta da var, senkronize olmalı

    void Awake()
    {
        if (slipRenderer == null) slipRenderer = GetComponent<Renderer>();
        if (slipRenderer != null)
        {
            // Başlangıçta materyalin bir kopyasını kullanmak iyi bir pratik olabilir
            // Böylece aynı materyali kullanan diğer objeler etkilenmez.
            // slipRenderer.material = new Material(slipRenderer.material); // İhtiyaç duyarsanız açın
            originalColor = slipRenderer.material.color;
            Debug.Log($"[Slip.Awake] {name} - originalColor BAŞLANGIÇTA: {originalColor} (Alpha: {originalColor.a})");
        }
        else Debug.LogWarning($"[Slip.Awake] {name} üzerinde Renderer eksik!");

        if (textMeshPro == null)
        {
            textMeshPro = GetComponentInChildren<TextMeshProUGUI>();
            if (textMeshPro == null) Debug.LogError($"[Slip.Awake] {name} üzerinde TextMeshProUGUI eksik/bulunamadı!");
        }

        // Rigidbody ve Collider ayarları (önceki önerilerdeki gibi olmalı)
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
            rb.isKinematic = false;
        }
        else Debug.LogError($"[Slip.Awake] {name} üzerinde Rigidbody eksik!");

        Collider genericCollider = GetComponent<Collider>();
        if (genericCollider != null)
        {
            genericCollider.isTrigger = false;
        }
        else Debug.LogError($"[Slip.Awake] {name} üzerinde herhangi bir Collider eksik!");

        // Etiket ve katman ayarları Unity Editor üzerinden yapılmalı.
        // gameObject.tag = "SlipTag"; 
        // gameObject.layer = LayerMask.NameToLayer("Slip"); 

        if (textMeshPro != null) textMeshPro.gameObject.SetActive(false);
    }

    public void SetOrderData(OrderData order)
    {
        orderData = order;
        if (orderData == null)
        {
            Debug.LogError($"[Slip.SetOrderData] {name} için OrderData null!");
            if (textMeshPro != null) textMeshPro.text = "HATA: Sipariş Verisi Yok";
            return;
        }
        UpdateText();

        // VERİ ATANDIKTAN SONRA RENGİ VE originalColor'I GÜVENCE ALTINA AL
        if (slipRenderer != null)
        {
            Color currentColor = slipRenderer.material.color;
            // Eğer başlangıç originalColor'ı şeffafsa veya şu anki renk şeffafsa, opak yap.
            if (originalColor.a < 0.9f || currentColor.a < 0.9f)
            {
                Debug.LogWarning($"[Slip.SetOrderData] {name} - Materyal alpha ({currentColor.a}) veya originalColor alpha ({originalColor.a}) düşük. Alpha 1.0 yapılıyor ve originalColor güncelleniyor.");
                currentColor.a = 1.0f; // Tamamen opak yap
                slipRenderer.material.color = currentColor; // Hemen uygula
                originalColor = currentColor; // Yeni, opak rengi originalColor olarak kaydet
            }
            Debug.Log($"[Slip.SetOrderData] {name} - originalColor SON DURUM: {originalColor} (Alpha: {originalColor.a})");
        }
    }

    public OrderData GetOrderData()
    {
        return orderData;
    }

    public void Highlight(bool highlight)
    {
        if (slipRenderer == null)
        {
            Debug.LogWarning($"[Slip.Highlight] {name} için slipRenderer null!");
            return;
        }

        Color highlightDisplayColor = Color.yellow; // Vurgu rengi
        highlightDisplayColor.a = 1.0f; // Vurgu renginin de opak olduğundan emin ol

        Color targetColor = highlight ? highlightDisplayColor : originalColor;

        // originalColor'ın alpha'sının da 1 olduğundan emin ol (SetOrderData'da yapılıyor ama burada da bir kontrol)
        if (!highlight && originalColor.a < 0.9f)
        {
            Debug.LogWarning($"[Slip.Highlight] {name} - Highlight KAPALI ama originalColor alpha ({originalColor.a}) düşük. Geçici olarak 1.0 yapılıyor.");
            Color tempOriginal = originalColor;
            tempOriginal.a = 1.0f;
            targetColor = tempOriginal;
        }

        slipRenderer.material.color = targetColor;
        // Debug.Log($"[Slip.Highlight] {name} - Highlight: {highlight}, Ayarlanan Renk: {targetColor} (Alpha: {targetColor.a}), Saklanan OriginalColor: {originalColor} (Alpha: {originalColor.a})");
    }

    public void OnPickedUp()
    {
        isHeld = true;
        if (textMeshPro != null) textMeshPro.gameObject.SetActive(true);
        // Debug.Log($"[Slip.OnPickedUp] Slip alındı: {name}");
        // Alındığında görünür olmalı, vurgulanabilir. Vurgu ObjectPickup'tan gelecek.
    }

    public void OnDropped()
    {
        isHeld = false;
        // Debug.Log($"[Slip.OnDropped] Slip bırakıldı: {name}");
        // Bırakıldığında ObjectPickup zaten SetActive(true) ve renderer.enabled = true yapıyor.
        // Vurgu kalktığında Highlight(false) ile originalColor'a dönecek.
    }

    private void UpdateText()
    {
        if (orderData == null || textMeshPro == null) return;

        StringBuilder builder = new StringBuilder();
        builder.AppendLine($"Sipariş ID: {orderData.orderID}");
        builder.AppendLine($"Müşteri: {orderData.customerName}");
        builder.AppendLine($"Tip: {orderData.orderType}");
        builder.AppendLine("<b>Ürünler:</b>");
        if (orderData.itemsInOrder != null && orderData.itemsInOrder.Count > 0)
        {
            foreach (var item in orderData.itemsInOrder)
            {
                string productName = "[Ürün Tanımsız]";
                if (item.productDefinition != null && !string.IsNullOrEmpty(item.productDefinition.productName))
                {
                    productName = item.productDefinition.productName;
                }
                builder.AppendLine($"- {productName} x{item.quantity}");
            }
        }
        else
        {
            builder.AppendLine("(Sipariş Kalemi Yok)");
        }
        builder.AppendLine($"<b>Toplam: {orderData.totalOrderValue:C2}</b>");

        textMeshPro.text = builder.ToString();
    }
}