using UnityEngine;
using System.Collections.Generic;

public class CargoBox : MonoBehaviour
{
    public bool isLargeBox;
    public float detectionRadius = 3f;
    public Transform[] productSlots;
    public OrderData assignedOrder;

    private List<Product> placedProducts = new List<Product>();
    private Rigidbody rb;
    private bool isBeingCarried = false;
    private Collider boxCollider;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        boxCollider = GetComponent<Collider>();
        gameObject.tag = "CargoBox";

        if (rb == null)
        {
            Debug.LogError("CargoBox’ta Rigidbody eksik!");
            return;
        }
        if (boxCollider == null)
        {
            Debug.LogError("CargoBox’ta Collider eksik!");
            return;
        }

        // Slot pozisyonlarını kontrol et ve log’a yaz
        for (int i = 0; i < productSlots.Length; i++)
        {
            if (productSlots[i] != null)
            {
                Debug.Log($"Slot {i} position: {productSlots[i].position}, scale: {productSlots[i].lossyScale}");
            }
            else
            {
                Debug.LogWarning($"Slot {i} is null!");
            }
        }

        Debug.Log($"CargoBox {gameObject.name} başlatıldı. isKinematic: {rb.isKinematic}");
    }

    public void AssignOrder(OrderData order)
    {
        assignedOrder = order;
        Debug.Log($"Order assigned to CargoBox {gameObject.name}: {order}");
    }

    public bool IsInRange(Vector3 productPosition)
    {
        return Vector3.Distance(transform.position, productPosition) <= detectionRadius;
    }

    public bool TryPlaceProduct(Product product)
    {
        if (placedProducts.Count >= productSlots.Length)
        {
            Debug.Log("Kargo kutusu dolu.");
            return false;
        }

        int slotIndex = placedProducts.Count;
        Transform slot = productSlots[slotIndex];
        if (slot == null)
        {
            Debug.LogError($"Hata: {gameObject.name} için slot {slotIndex} null!");
            return false;
        }

        Rigidbody productRb = product.GetComponent<Rigidbody>();
        if (productRb != null)
        {
            productRb.isKinematic = true;
            productRb.velocity = Vector3.zero;
            productRb.angularVelocity = Vector3.zero;
        }

        Collider productCollider = product.GetComponent<Collider>();
        if (productCollider != null && boxCollider != null)
        {
            Physics.IgnoreCollision(boxCollider, productCollider, true);
            productCollider.enabled = true;
            Debug.Log($"Çarpışma devre dışı bırakıldı: {product.gameObject.name} ile {gameObject.name}");
        }

        product.transform.SetParent(slot);
        product.transform.position = slot.position;
        product.transform.rotation = Quaternion.Euler(0, 0, 0);
        product.transform.localScale = Vector3.one;

        product.gameObject.SetActive(true);
        Renderer productRenderer = product.GetComponent<Renderer>();
        if (productRenderer != null)
            productRenderer.enabled = true;
        else
            Debug.LogWarning($"Product {product.name} has no Renderer component!");

        placedProducts.Add(product);
        Debug.Log($"{product.gameObject.name} kargo kutusuna yerleştirildi. Pozisyon: {product.transform.position}, active = {product.gameObject.activeSelf}");

        return true;
    }

    public Product TryRemoveProduct(Vector3 rayOrigin, Vector3 rayDirection, float maxDistance)
    {
        if (isBeingCarried)
        {
            Debug.Log("Kutu taşınıyor, ürün alınamaz!");
            return null;
        }

        LayerMask pickupLayer = LayerMask.GetMask("Pickup");
        Ray ray = new Ray(rayOrigin, rayDirection);
        if (Physics.Raycast(ray, out RaycastHit hit, maxDistance, pickupLayer))
        {
            Product product = hit.collider.GetComponent<Product>();
            if (product != null && placedProducts.Contains(product))
            {
                placedProducts.Remove(product);
                product.transform.SetParent(null);

                Collider productCollider = product.GetComponent<Collider>();
                if (productCollider != null && boxCollider != null)
                {
                    Physics.IgnoreCollision(boxCollider, productCollider, false);
                    Debug.Log($"Çarpışma etkinleştirildi: {product.gameObject.name} ile {gameObject.name}");
                }

                Rigidbody productRb = product.GetComponent<Rigidbody>();
                if (productRb != null)
                {
                    productRb.isKinematic = false;
                    productRb.velocity = Vector3.zero;
                    productRb.angularVelocity = Vector3.zero;
                }

                product.transform.localScale = product.GetOriginalScale();

                product.gameObject.SetActive(true);
                Renderer productRenderer = product.GetComponent<Renderer>();
                if (productRenderer != null)
                    productRenderer.enabled = true;

                Debug.Log($"{product.gameObject.name} kargo kutusundan alındı. Ölçek: {product.transform.localScale}, active = {product.gameObject.activeSelf}");
                return product;
            }
            else
            {
                Debug.Log("Raycast bir Product’a çarptı, ancak bu ürün kutuda değil.");
            }
        }
        else
        {
            Debug.Log("Raycast hiçbir şeye çarpmadı.");
        }
        return null;
    }

    public bool IsFull()
    {
        return placedProducts.Count >= productSlots.Length;
    }

    public void OnPickedUp()
    {
        isBeingCarried = true;
        rb.isKinematic = true;
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        Debug.Log($"CargoBox {gameObject.name} alındı.");
    }

    public void OnDropped()
    {
        isBeingCarried = false;
        rb.isKinematic = false;
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        Debug.Log($"CargoBox {gameObject.name} bırakıldı. isKinematic: {rb.isKinematic}, Velocity: {rb.velocity}, AngularVelocity: {rb.angularVelocity}");
    }

    public bool IsBeingCarried()
    {
        return isBeingCarried;
    }
}