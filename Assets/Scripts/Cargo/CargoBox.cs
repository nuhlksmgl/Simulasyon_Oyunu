using UnityEngine;
using System.Collections.Generic;

public class CargoBox : MonoBehaviour
{
    public bool isLargeBox;
    public float detectionRadius = 3f;
    public Transform[] productSlots;

    private List<Product> placedProducts = new List<Product>();
    private Rigidbody rb;
    private bool isBeingCarried = false; // Kutu taşınıyor mu?
    private Collider boxCollider; // Kendi Collider’ımız

    void Awake()
    {
        // Rigidbody ve Collider’ı al
        rb = GetComponent<Rigidbody>();
        boxCollider = GetComponent<Collider>();

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

        // Başlangıçta isKinematic zaten true (Inspector’da ayarlı)
        Debug.Log($"CargoBox {gameObject.name} başlatıldı. isKinematic: {rb.isKinematic}");
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

        Rigidbody productRb = product.GetComponent<Rigidbody>();
        if (productRb != null)
        {
            productRb.isKinematic = true;
            productRb.velocity = Vector3.zero;
            productRb.angularVelocity = Vector3.zero;
        }

        // Ürünün Collider’ını kutunun Collider’ıyla çarpışmayacak şekilde ayarla
        Collider productCollider = product.GetComponent<Collider>();
        if (productCollider != null && boxCollider != null)
        {
            Physics.IgnoreCollision(boxCollider, productCollider, true);
            Debug.Log($"Çarpışma devre dışı bırakıldı: {product.gameObject.name} ile {gameObject.name}");
        }

        // Ürünü slota yerleştir, rotasyonu sabitle
        product.transform.SetParent(slot);
        product.transform.position = slot.position;
        product.transform.rotation = Quaternion.Euler(0, 0, 0); // Rotasyonu sıfırla

        Debug.Log($"{product.gameObject.name} kargo kutusuna yerleştirildikten sonra rotasyon: {product.transform.rotation.eulerAngles}");

        placedProducts.Add(product);
        Debug.Log($"{product.gameObject.name} kargo kutusuna yerleştirildi.");

        return true;
    }

    public Product TryRemoveProduct(Vector3 rayOrigin, Vector3 rayDirection, float maxDistance)
    {
        Ray ray = new Ray(rayOrigin, rayDirection);
        if (Physics.Raycast(ray, out RaycastHit hit, maxDistance))
        {
            Product product = hit.collider.GetComponent<Product>();
            if (product != null && placedProducts.Contains(product))
            {
                placedProducts.Remove(product);
                product.transform.SetParent(null);

                // Çarpışmayı tekrar etkinleştir
                Collider productCollider = product.GetComponent<Collider>();
                if (productCollider != null && boxCollider != null)
                {
                    Physics.IgnoreCollision(boxCollider, productCollider, false);
                    Debug.Log($"Çarpışma etkinleştirildi: {product.gameObject.name} ile {gameObject.name}");
                }

                Debug.Log($"{product.gameObject.name} kargo kutusundan alındı.");
                return product;
            }
        }
        return null;
    }

    public bool IsFull()
    {
        return placedProducts.Count >= productSlots.Length;
    }

    // Kutu alındığında çağrılır
    public void OnPickedUp()
    {
        isBeingCarried = true;
        rb.isKinematic = true; // Zaten true olabilir, ama emin olmak için
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        Debug.Log($"CargoBox {gameObject.name} alındı.");
    }

    // Kutu bırakıldığında çağrılır
    public void OnDropped()
    {
        isBeingCarried = false;
        rb.isKinematic = false; // Fiziksel etkileşimleri aç

        // Hız ve açısal hızı sıfırla
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        Debug.Log($"CargoBox {gameObject.name} bırakıldı. isKinematic: {rb.isKinematic}, Velocity: {rb.velocity}, AngularVelocity: {rb.angularVelocity}");
    }

    public bool IsBeingCarried()
    {
        return isBeingCarried;
    }
}