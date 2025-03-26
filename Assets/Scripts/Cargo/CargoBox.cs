using UnityEngine;
using System.Collections.Generic;

public class CargoBox : MonoBehaviour
{
    public bool isLargeBox;
    public float detectionRadius = 3f;
    public Transform[] productSlots;

    private List<Product> placedProducts = new List<Product>();

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

        Rigidbody rb = product.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        // Ürünü slota yerleştir, rotasyonu sabitle
        product.transform.SetParent(slot);
        product.transform.position = slot.position;
        product.transform.rotation = Quaternion.Euler(0, 0, 0); // Rotasyonu sıfırla (siparişle gelenler gibi)

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
}