using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class CargoBox : MonoBehaviour
{
    [Header("Genel Kutu Ayarları")]
    public bool isLargeBox;
    [SerializeField] private Transform[] productSlots;
    public OrderData assignedOrder;

    [Header("Animasyon Ayarları")]
    [Tooltip("Root objenin üzerindeki Animator component'ini buraya sürükleyin.")]
    [SerializeField] private Animator boxAnimator;

    public bool IsOpen { get; private set; } = false;

    private List<Product> placedProducts = new List<Product>();
    private bool[] slotOccupied;
    private Rigidbody rb;
    private bool isBeingCarried = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null) Debug.LogError($"CargoBox ({gameObject.name}): Rigidbody component'i eksik!");
        if (boxAnimator == null) Debug.LogError($"CargoBox ({gameObject.name}): Box Animator atanmamış!");

        if (productSlots != null && productSlots.Length > 0)
        {
            slotOccupied = new bool[productSlots.Length];
        }
    }

    public void ToggleLids()
    {
        IsOpen = !IsOpen;
        if (boxAnimator != null)
        {
            boxAnimator.SetBool("IsOpen", IsOpen);
        }
    }

    public void SetLidStateForced(bool open)
    {
        IsOpen = open;
        if (boxAnimator != null)
        {
            boxAnimator.SetBool("IsOpen", open);
            if (open) boxAnimator.Play("Open_State", 0, 1f);
            else boxAnimator.Play("Closed_State", 0, 1f);
        }
    }

    public bool TryPlaceProduct(Product product)
    {
        if (!IsOpen)
        {
            Debug.LogWarning($"Kutu kapalı ({gameObject.name}), ürün yerleştirilemez.");
            return false;
        }

        if (product == null || IsFull() || (!isLargeBox && product.productDefinition.isLarge))
        {
            return false;
        }

        int slotIndex = -1;
        for (int i = 0; i < slotOccupied.Length; i++)
        {
            if (!slotOccupied[i])
            {
                slotIndex = i;
                break;
            }
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
        if (!IsOpen) return null;
        if (isBeingCarried) return null;

        // Işını kullanarak spesifik ürünü bul ve döndür
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

            return productToTake;
        }
        return null;
    }

    // YENİ YARDIMCI FONKSİYON
    public Product GetProductAtRay(Ray ray, float maxDistance)
    {
        if (!IsOpen) return null;

        // Işının çarptığı tüm objeleri al
        RaycastHit[] hits = Physics.RaycastAll(ray, maxDistance);

        // Bu kutudaki ürünlerle eşleşen en yakın olanı bul
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

    public void AssignOrder(OrderData order) => assignedOrder = order;
    public bool IsFull() => productSlots != null && placedProducts.Count >= productSlots.Length;
    public void OnPickedUp() { isBeingCarried = true; if (rb != null) rb.isKinematic = true; }
    public void OnDropped() { isBeingCarried = false; if (rb != null) rb.isKinematic = false; }
    public bool IsBeingCarried() => isBeingCarried;
}