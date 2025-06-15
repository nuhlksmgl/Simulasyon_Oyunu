using UnityEngine;
using System.Collections.Generic;
using System.Linq;

// Eğer DataModels.cs dosyanız bir namespace içindeyse, onu buraya ekleyin.
// Örnek: using Simulasyon.Data;

public class CargoBox : MonoBehaviour
{
    [Header("Genel Kutu Ayarları")]
    public bool isLargeBox;
    [Tooltip("Kutunun içindeki ürünlerin yerleşeceği boş Transform nesneleri.")]
    [SerializeField] private Transform[] productSlots;
    public OrderData assignedOrder;

    [Header("Animasyon Ayarları")]
    [Tooltip("Bu obje üzerindeki Animator component'ini buraya sürükleyin.")]
    [SerializeField] private Animator boxAnimator;

    public bool IsOpen { get; private set; } = false;

    private List<Product> placedProducts = new List<Product>();
    private bool[] slotOccupied;
    private Rigidbody rb;
    private bool isBeingCarried = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null) Debug.LogError("Rigidbody component'i eksik!", this.gameObject);
        if (boxAnimator == null) Debug.LogError("Box Animator atanmamış!", this.gameObject);
        if (productSlots != null && productSlots.Length > 0)
        {
            slotOccupied = new bool[productSlots.Length];
        }
    }

    // =================================================================
    // =========== YENİ EKLENEN OTOMATİK DOLDURMA METODU ==============
    // =================================================================

    /// <summary>
    /// DeliveryManager tarafından çağrılarak kutunun içini otomatik olarak doldurur.
    /// </summary>
    public void InitializeBox(List<MarketProduct> itemsToPlace)
    {
        // --- KONTROL 5 ---
        Debug.Log($"5. KUTU İÇİ DOLDURULUYOR: {gameObject.name} kutusuna {itemsToPlace.Count} adet ürün yerleştirme komutu alındı.");

        // Ürünleri yerleştirmeden önce kutu kapaklarını (animasyon için) açık duruma getirelim.
        SetLidStateForced(true);

        foreach (var productData in itemsToPlace)
        {
            if (productData.productPrefab != null)
            {
                // 1. Ürünün fiziksel kopyasını oluştur (prefab'dan)
                GameObject productObj = Instantiate(productData.productPrefab);

                // 2. Oluşturulan kopyanın üzerindeki 'Product' script'ini al
                Product productScript = productObj.GetComponent<Product>();

                if (productScript != null)
                {
                    // 3. Entegrasyonun kilit noktası: Yeni ürüne, hangi market verisine ait olduğunu söylüyoruz.
                    productScript.productDefinition = productData;

                    // 4. Mevcut 'TryPlaceProduct' metodunu kullanarak ürünü kutuya yerleştir.
                    bool success = TryPlaceProduct(productScript);
                    if (!success)
                    {
                        Debug.LogError($"{productData.productName} kutuya yerleştirilemedi! Kutu dolu veya ürün boyutu uygun değil.", this.gameObject);
                        Destroy(productObj); // Yerleştirilemezse objeyi yok et.
                    }
                }
            }
        }
        // Tüm ürünler yerleştirildikten sonra kapakları kapat.
        SetLidStateForced(false);
    }

    // =================================================================
    // =========== MEVCUT METOTLARINIZ (DEĞİŞİKLİK YOK) =================
    // =================================================================

    public void ToggleLids()
    {
        IsOpen = !IsOpen;
        if (boxAnimator != null) boxAnimator.SetBool("IsOpen", IsOpen);
    }

    public void SetLidStateForced(bool open)
    {
        IsOpen = open;
        if (boxAnimator != null)
        {
            boxAnimator.SetBool("IsOpen", open);
            // Animasyonun doğrudan son karesine gitmesini sağlar
            if (open) boxAnimator.Play("Open_State", 0, 1f);
            else boxAnimator.Play("Closed_State", 0, 1f);
        }
    }

    public void AssignOrder(OrderData order)
    {
        assignedOrder = order;
    }

    public bool TryPlaceProduct(Product product)
    {
        if (product == null || product.productDefinition == null)
        {
            Debug.LogError("TryPlaceProduct'a gelen ürün veya productDefinition null!", this.gameObject);
            return false;
        }

        if (!IsOpen || IsFull() || (!isLargeBox && product.productDefinition.isLarge)) return false;

        int slotIndex = -1;
        for (int i = 0; i < slotOccupied.Length; i++)
        {
            if (!slotOccupied[i]) { slotIndex = i; break; }
        }
        if (slotIndex == -1) return false;

        Transform slot = productSlots[slotIndex];

        product.transform.SetParent(slot);
        product.transform.localPosition = Vector3.zero;
        product.transform.localRotation = Quaternion.identity;

        Vector3 originalWorldScale = product.GetOriginalWorldScale();
        Vector3 parentWorldScale = slot.lossyScale;
        product.transform.localScale = new Vector3(
            originalWorldScale.x / (parentWorldScale.x == 0 ? 1 : parentWorldScale.x),
            originalWorldScale.y / (parentWorldScale.y == 0 ? 1 : parentWorldScale.y),
            originalWorldScale.z / (parentWorldScale.z == 0 ? 1 : parentWorldScale.z)
        );

        if (product.TryGetComponent<Rigidbody>(out var productRb)) productRb.isKinematic = true;

        placedProducts.Add(product);
        slotOccupied[slotIndex] = true;
        return true;
    }

    public Product TryRemoveProduct(Vector3 rayOrigin, Vector3 rayDirection, float maxDistance)
    {
        if (!IsOpen || isBeingCarried) return null;
        Product productToTake = GetProductAtRay(new Ray(rayOrigin, rayDirection), maxDistance);
        if (productToTake != null)
        {
            placedProducts.Remove(productToTake);
            int slotIndexToFree = -1;
            for (int i = 0; i < productSlots.Length; i++)
            {
                if (productSlots[i] == productToTake.transform.parent)
                {
                    slotIndexToFree = i;
                    break;
                }
            }
            if (slotIndexToFree != -1) slotOccupied[slotIndexToFree] = false;

            productToTake.transform.SetParent(null);
            productToTake.transform.localScale = productToTake.GetOriginalWorldScale();
            if (productToTake.TryGetComponent<Rigidbody>(out Rigidbody productRb))
            {
                productRb.isKinematic = false;
            }
            return productToTake;
        }
        return null;
    }

    public Product GetProductAtRay(Ray ray, float maxDistance)
    {
        if (!IsOpen) return null;
        RaycastHit[] hits = Physics.RaycastAll(ray, maxDistance);
        Product closestProduct = null;
        float minDistance = float.MaxValue;
        foreach (RaycastHit hit in hits)
        {
            Product productInBox = hit.collider.GetComponent<Product>();
            if (productInBox != null && placedProducts.Contains(productInBox))
            {
                if (hit.distance < minDistance)
                {
                    minDistance = hit.distance;
                    closestProduct = productInBox;
                }
            }
        }
        return closestProduct;
    }

    public bool IsFull() => productSlots != null && placedProducts.Count >= productSlots.Length;
    public void OnPickedUp() { isBeingCarried = true; if (rb != null) rb.isKinematic = true; }
    public void OnDropped() { isBeingCarried = false; if (rb != null) rb.isKinematic = false; }
    public bool IsBeingCarried() => isBeingCarried;
}