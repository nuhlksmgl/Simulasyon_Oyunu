using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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

        // Ürün yerleştirme işlemi Product scriptini yok etmez, sadece ebeveynini değiştirir.
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

        foreach (Product productInBox in placedProducts)
        {
            // Basitleştirilmiş mantık: Şimdilik kutudaki herhangi bir ürünü al
            // Daha gelişmiş bir sistem için ışınla spesifik ürünü hedefleyebilirsiniz.
            placedProducts.Remove(productInBox);

            int slotIndex = -1;
            for (int i = 0; i < productSlots.Length; i++)
            {
                if (productSlots[i] == productInBox.transform.parent)
                {
                    slotIndex = i;
                    break;
                }
            }
            if (slotIndex != -1) slotOccupied[slotIndex] = false;

            return productInBox;
        }
        return null;
    }

    public void AssignOrder(OrderData order) => assignedOrder = order;
    public bool IsFull() => productSlots != null && placedProducts.Count >= productSlots.Length;
    public void OnPickedUp() { isBeingCarried = true; if (rb != null) rb.isKinematic = true; }
    public void OnDropped() { isBeingCarried = false; if (rb != null) rb.isKinematic = false; }
    public bool IsBeingCarried() => isBeingCarried;
}