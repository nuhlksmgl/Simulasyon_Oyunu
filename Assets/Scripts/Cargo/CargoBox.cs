using UnityEngine;
using System.Collections.Generic;

public class CargoBox : MonoBehaviour
{
    public bool isLargeBox; // Büyük veya küçük kutu durumu
    public float detectionRadius = 3f; // Algılama mesafesi
    public Transform[] productSlots; // Ürünlerin yerleşeceği slotların referansları

    private List<Product> placedProducts = new List<Product>(); // Yerleştirilen ürünlerin listesi

    // Ürünün kutuya yakın olup olmadığını kontrol eder
    public bool IsInRange(Vector3 productPosition)
    {
        return Vector3.Distance(transform.position, productPosition) <= detectionRadius;
    }

    // Ürünü kutuya yerleştirmeyi dener
    public bool TryPlaceProduct(Product product)
    {
        if (placedProducts.Count >= productSlots.Length)
        {
            Debug.Log("Kargo kutusu dolu.");
            return false; // Kutu doluysa ürünü yerleştirme
        }

        // Ürünü boş slota yerleştir
        int slotIndex = placedProducts.Count;
        Transform slot = productSlots[slotIndex];

        product.transform.SetParent(slot); // Ürünü slotun child'ı yap
        product.transform.localPosition = Vector3.zero; // Slotun pozisyonuna göre yerleştir
        product.transform.localRotation = Quaternion.identity; // Slotun yönüyle hizala

        placedProducts.Add(product); // Ürünü yerleştirilen ürünler listesine ekle
        Debug.Log($"{product.gameObject.name} kargo kutusuna yerleştirildi.");

        return true;
    }

    // Kutudan bir ürünü geri almak için yöntem
    public Product TryRemoveProduct(Vector3 rayOrigin, Vector3 rayDirection, float maxDistance)
    {
        Ray ray = new Ray(rayOrigin, rayDirection);
        if (Physics.Raycast(ray, out RaycastHit hit, maxDistance))
        {
            Product product = hit.collider.GetComponent<Product>();
            if (product != null && placedProducts.Contains(product))
            {
                placedProducts.Remove(product); // Listeden çıkar
                product.transform.SetParent(null); // Slotun parent'lığından çıkar
                Debug.Log($"{product.gameObject.name} kargo kutusundan alındı.");
                return product;
            }
        }
        return null;
    }

    // Kutunun dolu olup olmadığını kontrol eder
    public bool IsFull()
    {
        return placedProducts.Count >= productSlots.Length;
    }
}