using UnityEngine;
using System.Collections.Generic;

public class CargoBox : MonoBehaviour
{
    public bool isLargeBox; // Büyük veya küçük kutu durumu
    public float detectionRadius = 3f; // Algılama mesafesi
    public Transform[] productSlots; // Ürünlerin yerleşeceği slotların referansları
    public GameObject fullBoxPanel; // Kargo kutusu dolunca açılacak panel

    private List<Product> placedProducts = new List<Product>(); // Yerleştirilen ürünlerin listesi

    void Start()
    {
        if (fullBoxPanel != null)
        {
            fullBoxPanel.SetActive(false); // Paneli başlangıçta kapalı yap
        }
    }

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
            if (fullBoxPanel != null)
            {
                fullBoxPanel.SetActive(true); // Paneli aç
            }
            return false; // Kutu doluysa ürünü yerleştirme
        }

        // Ürünü boş slota yerleştir
        int slotIndex = placedProducts.Count; // Mevcut ürün sayısı, yerleştirilecek slotu gösterir
        Transform slot = productSlots[slotIndex];

        product.transform.SetParent(slot); // Ürünü slotun child'ı yap
        product.transform.localPosition = Vector3.zero; // Slotun pozisyonuna göre yerleştir
        product.transform.localRotation = Quaternion.identity; // Slotun yönüyle hizala

        placedProducts.Add(product); // Ürünü yerleştirilen ürünler listesine ekle
        Debug.Log($"{product.gameObject.name} kargo kutusuna yerleştirildi.");

        // Kargo kutusu dolmuşsa paneli aç
        if (placedProducts.Count >= productSlots.Length)
        {
            if (fullBoxPanel != null)
            {
                fullBoxPanel.SetActive(true);
            }
        }

        return true;
    }
}
