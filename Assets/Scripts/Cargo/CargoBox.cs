// FileName: CargoBox.cs
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class CargoBox : MonoBehaviour
{
    [Header("Genel Kutu Ayarları")]
    public Transform slipSlot;
    public bool isLargeBox;
    [SerializeField] private Transform[] productSlots;
    public OrderData assignedOrder;

    [Header("Animasyon Ayarları")]
    [SerializeField] private Animator boxAnimator;

    public bool IsOpen { get; private set; } = false;
    private List<Product> placedProducts = new List<Product>();
    private bool[] slotOccupied;
    private Rigidbody rb;
    private bool isBeingCarried = false;

    /// <summary>
    /// Bu kutunun geçerli bir siparişe sahip olup olmadığını kontrol eder.
    /// Sadece siparişin varlığına değil, ID'sinin de dolu olmasına bakar.
    /// </summary>
    public bool HasValidAssignedOrder()
    {
        return assignedOrder != null && !string.IsNullOrWhiteSpace(assignedOrder.orderID);
    }

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

    public void AttachSlip(Slip slip)
    {
        if (HasValidAssignedOrder() || slip == null)
        {
            Debug.LogError($"YAPIŞTIRMA BAŞARISIZ! '{this.name}' kutusu zaten geçerli bir siparişe sahip (ID: {assignedOrder?.orderID}) veya gelen slip boş.", this.gameObject);
            return;
        }

        AssignOrder(slip.GetOrderData());

        if (HasValidAssignedOrder())
        {
            slip.transform.SetParent(slipSlot);
            slip.transform.SetPositionAndRotation(slipSlot.position, slipSlot.rotation);
            slip.transform.localScale = slip.GetOriginalScale();
            if (slip.TryGetComponent<Rigidbody>(out var slipRb)) slipRb.isKinematic = true;
            if (slip.TryGetComponent<Collider>(out var slipCol)) slipCol.enabled = false;
            slip.OnDropped();
            Debug.Log($"BAŞARILI: {assignedOrder.orderID} ID'li sipariş '{this.name}' kutusuna atandı.", this.gameObject);
        }
        else
        {
            Debug.LogError("KRİTİK HATA: AssignOrder çağrıldıktan sonra bile 'assignedOrder' alanı hala geçersiz!", this.gameObject);
        }
    }

    public float CalculatePackingPenalty()
    {
        if (!HasValidAssignedOrder() || assignedOrder.itemsInOrder == null) return -10f;
        var requiredItems = assignedOrder.itemsInOrder.GroupBy(i => i.productDefinition.productName).ToDictionary(g => g.Key, g => g.Sum(i => i.quantity));
        var placedItems = placedProducts.GroupBy(p => p.productDefinition.productName).ToDictionary(g => g.Key, g => g.Count());
        float penalty = 0f;
        float basePenalty = CustomerOrderManager.Instance != null ? CustomerOrderManager.Instance.reputationForFailure : -3.0f;
        var allItemKeys = requiredItems.Keys.Union(placedItems.Keys).ToList();

        foreach (var key in allItemKeys)
        {
            int required = requiredItems.GetValueOrDefault(key, 0);
            int placed = placedItems.GetValueOrDefault(key, 0);
            if (placed < required) penalty += basePenalty * (required - placed);
            if (placed > required) penalty += (basePenalty / 2) * (placed - required);
        }
        return penalty;
    }

    public bool TryPlaceProduct(Product product)
    {
        if (product == null || product.productDefinition == null) return false;
        if (!IsOpen || IsFull() || (!isLargeBox && product.productDefinition.isLarge)) return false;
        int slotIndex = -1;
        for (int i = 0; i < slotOccupied.Length; i++) if (!slotOccupied[i]) { slotIndex = i; break; }
        if (slotIndex == -1) return false;

        Transform slot = productSlots[slotIndex];
        product.transform.SetParent(slot);
        product.transform.SetPositionAndRotation(slot.position, slot.rotation);

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

    public void InitializeBox(List<MarketProduct> itemsToPlace)
    {
        SetLidStateForced(true);
        foreach (var productData in itemsToPlace)
        {
            if (productData.productPrefab != null)
            {
                GameObject productObj = Instantiate(productData.productPrefab);
                Product productScript = productObj.GetComponent<Product>();
                if (productScript != null)
                {
                    productScript.productDefinition = productData;
                    if (!TryPlaceProduct(productScript))
                    {
                        Debug.LogError($"{productData.productName} kutuya yerleştirilemedi!", gameObject);
                        Destroy(productObj);
                    }
                }
            }
        }
        SetLidStateForced(false);
    }

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
            if (open) boxAnimator.Play("Open_State", 0, 1f);
            else boxAnimator.Play("Closed_State", 0, 1f);
        }
    }

    public void AssignOrder(OrderData order)
    {
        assignedOrder = order;
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
                if (productSlots[i] == productToTake.transform.parent) { slotIndexToFree = i; break; }
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
        var hits = Physics.RaycastAll(ray, maxDistance);
        Product closestProduct = null;
        float minDistance = float.MaxValue;
        foreach (var hit in hits)
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

    public bool IsFull()
    {
        return productSlots != null && placedProducts.Count >= productSlots.Length;
    }

    public void OnPickedUp()
    {
        isBeingCarried = true;
        if (rb != null) rb.isKinematic = true;
    }

    public void OnDropped()
    {
        isBeingCarried = false;
        if (rb != null) rb.isKinematic = false;
    }

    public bool IsBeingCarried()
    {
        return isBeingCarried;
    }
}