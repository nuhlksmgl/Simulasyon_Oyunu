using UnityEngine;
using TMPro;
using System.Text;

public class Slip : MonoBehaviour
{
    private OrderData orderData;
    [SerializeField] private Renderer slipRenderer;
    [SerializeField] private TextMeshProUGUI textMeshPro; // World Space Canvas’taki TextMeshPro
    private Color originalColor;
    private bool isHeld; // Bu değişken artık ObjectPickup tarafından yönetiliyor gibi görünüyor,
                         // ama slip'in kendi iç mantığı için tutulabilir.

    void Awake()
    {
        if (slipRenderer == null) slipRenderer = GetComponent<Renderer>();
        if (slipRenderer != null)
        {
            originalColor = slipRenderer.material.color;
        }
        else
        {
            Debug.LogWarning($"Slip {name} üzerinde Renderer eksik!");
        }

        if (textMeshPro == null)
        {
            // Eğer TextMeshProUGUI objesi bu GameObject'in bir alt objesi ise,
            // onu otomatik olarak bulmayı deneyebilirsiniz:
            textMeshPro = GetComponentInChildren<TextMeshProUGUI>();
            if (textMeshPro == null)
            {
                Debug.LogError($"Slip {name} üzerinde veya alt objelerinde TextMeshProUGUI bulunamadı! Lütfen Inspector'dan atayın.");
            }
        }

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.collisionDetectionMode = CollisionDetectionMode.Discrete; // Veya ContinuousSpeculative daha iyi olabilir
            rb.isKinematic = false; // Başlangıçta kinematik olmamalı, fiziksel olarak düşebilmeli
            // rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
            // Yukarıdaki satır, slip'in serbestken kendi kendine dönmesini engellemek için kullanılabilir.
            // ObjectPickup script'i zaten tutarken dönüşü kontrol ediyor.
        }
        else
        {
            Debug.LogError($"Slip {name} üzerinde Rigidbody eksik! Fiziksel etkileşimler için gereklidir.");
        }

        BoxCollider collider = GetComponent<BoxCollider>();
        if (collider != null)
        {
            collider.isTrigger = false; // Fiziksel çarpışmalar için trigger olmamalı
        }
        else
        {
            Debug.LogError($"Slip {name} üzerinde BoxCollider eksik! Etkileşimler için gereklidir.");
        }

        // !!! ÖNEMLİ DEĞİŞİKLİK: Aşağıdaki satırları kaldırın veya yorum satırı yapın !!!
        // Bu satırlar, ObjectPickup script'inin slip'i doğru tanımasını engelliyordu.
        // Etiket ve Katman ayarlarını Unity Editor üzerinden yapın:
        // Slip prefab/objesinin Tag'ini "SlipTag" yapın.
        // Slip prefab/objesinin Layer'ını "Slip" (veya ObjectPickup'ta tanımladığınız slip katmanı) yapın.
        // gameObject.tag = "Pickup";
        // gameObject.layer = LayerMask.NameToLayer("Pickup");

        // Başlangıçta yazı görünmez
        if (textMeshPro != null)
        {
            textMeshPro.gameObject.SetActive(false);
        }
    }

    public void SetOrderData(OrderData order)
    {
        orderData = order;
        if (orderData == null)
        {
            Debug.LogError($"Slip {name} için atanan OrderData null!");
            if (textMeshPro != null) textMeshPro.text = "HATA: Sipariş Bilgisi Yok";
            return;
        }
        UpdateText(); // OrderData atandığında yazıyı güncelle
    }

    public OrderData GetOrderData()
    {
        return orderData;
    }

    // Bu Highlight metodu ObjectPickup script'i tarafından çağrılacak.
    public void Highlight(bool highlight)
    {
        if (slipRenderer == null) return;
        // ObjectPickup script'indeki highlightColor'ı kullanmak yerine
        // burada spesifik bir renk (örneğin sarı) kullanabilirsiniz
        // ya da ObjectPickup'taki rengi parametre olarak alabilirsiniz.
        // Şimdilik sarı olarak bırakıyorum.
        slipRenderer.material.color = highlight ? Color.yellow : originalColor;
        Debug.Log($"Slip {name} Highlight: {highlight}");
    }

    public void OnPickedUp()
    {
        isHeld = true;
        if (textMeshPro != null)
        {
            textMeshPro.gameObject.SetActive(true); // Alındığında yazıyı göster
        }
        Debug.Log($"Slip alındı: {name}, Sipariş ID {(orderData != null ? orderData.orderID : "Bilinmiyor")}");
    }

    public void OnDropped()
    {
        isHeld = false;
        // Slip bırakıldığında yazının görünür kalıp kalmayacağı size bağlı.
        // if (textMeshPro != null) textMeshPro.gameObject.SetActive(false);
        Debug.Log($"Slip bırakıldı: {name}, Sipariş ID {(orderData != null ? orderData.orderID : "Bilinmiyor")}");
    }

    private void UpdateText()
    {
        if (orderData == null || textMeshPro == null)
        {
            if (textMeshPro != null && orderData == null) textMeshPro.text = "Sipariş bilgisi bekleniyor...";
            return;
        }

        StringBuilder builder = new StringBuilder();
        builder.AppendLine($"Sipariş ID: {orderData.orderID}");
        builder.AppendLine($"Müşteri: {orderData.customerName}");
        builder.AppendLine($"Tip: {orderData.orderType}");
        builder.AppendLine("<b>Ürünler:</b>"); // Kalın font için Rich Text
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
        builder.AppendLine($"<b>Toplam: {orderData.totalOrderValue:C2}</b>"); // Para birimi formatı için :C2

        textMeshPro.text = builder.ToString();
    }

    // İsteğe bağlı: Eğer slip'ler fiziksel olarak bir yere çarpıp ses çıkarması
    // veya başka bir etkileşimde bulunması gerekiyorsa OnCollisionEnter vb. metodlar eklenebilir.
    // private void OnCollisionEnter(Collision collision)
    // {
    //     if (!isHeld) // Sadece elde değilken
    //     {
    //         Debug.Log($"{name} bir şeye çarptı: {collision.gameObject.name}");
    //     }
    // }
}