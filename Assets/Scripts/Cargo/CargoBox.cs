using UnityEngine;
using System.Collections.Generic;

public class CargoBox : MonoBehaviour
{
    public bool isLargeBox; // B�y�k veya k���k kutu durumu
    public float detectionRadius = 3f; // Alg�lama mesafesi
    public Transform[] productSlots; // �r�nlerin yerle�ece�i slotlar�n referanslar�

    private List<Product> placedProducts = new List<Product>(); // Yerle�tirilen �r�nlerin listesi

    // �r�n�n kutuya yak�n olup olmad���n� kontrol eder
    public bool IsInRange(Vector3 productPosition)
    {
        return Vector3.Distance(transform.position, productPosition) <= detectionRadius;
    }

    // �r�n� kutuya yerle�tirmeyi dener
    public bool TryPlaceProduct(Product product)
    {
        if (placedProducts.Count >= productSlots.Length)
        {
            Debug.Log("Kargo kutusu dolu.");
            return false; // Kutu doluysa �r�n� yerle�tirme
        }

        // �r�n� bo� slota yerle�tir
        int slotIndex = placedProducts.Count; // Mevcut �r�n say�s�, yerle�tirilecek slotu g�sterir
        Transform slot = productSlots[slotIndex];

        product.transform.SetParent(slot); // �r�n� slotun child'� yap
        product.transform.localPosition = Vector3.zero; // Slotun pozisyonuna g�re yerle�tir
        product.transform.localRotation = Quaternion.identity; // Slotun y�n�yle hizala

        placedProducts.Add(product); // �r�n� yerle�tirilen �r�nler listesine ekle
        Debug.Log($"{product.gameObject.name} kargo kutusuna yerle�tirildi.");

        return true;
    }
}
