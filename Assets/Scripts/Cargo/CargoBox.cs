using UnityEngine;
using System.Collections.Generic;

public class CargoBox : MonoBehaviour
{
    public bool isLargeBox;
    [SerializeField] private Transform[] productSlots;
    public OrderData assignedOrder;

    private List<Product> placedProducts = new List<Product>();
    private bool[] slotOccupied;
    private Rigidbody rb;
    private bool isBeingCarried = false;
    private Collider boxCollider;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        boxCollider = GetComponent<Collider>();
        gameObject.tag = "CargoBox";

        if (rb == null) Debug.LogError($"CargoBox ({gameObject.name}): Rigidbody component'i eksik!");
        if (boxCollider == null) Debug.LogError($"CargoBox ({gameObject.name}): Collider component'i eksik!");
        if (productSlots == null || productSlots.Length == 0)
        {
            Debug.LogError($"CargoBox ({gameObject.name}): ProductSlots dizisi atanmamış veya boş! Lütfen Unity Inspector'da productSlots dizisini kontrol edin.");
        }
        else
        {
            slotOccupied = new bool[productSlots.Length]; // Slot işgal durumunu başlat
            for (int i = 0; i < productSlots.Length; i++)
            {
                if (productSlots[i] == null)
                    Debug.LogError($"CargoBox ({gameObject.name}): ProductSlots[{i}] null olarak atanmış!");
                else
                {
                    Debug.Log($"Slot {i} pozisyonu: {productSlots[i].position}, dünya ölçeği: {productSlots[i].lossyScale}, yerel ölçek: {productSlots[i].localScale}");
                    // Slot pozisyonlarının farklı olduğunu kontrol et
                    for (int j = i + 1; j < productSlots.Length; j++)
                    {
                        if (productSlots[j] != null && productSlots[i].position == productSlots[j].position)
                        {
                            Debug.LogWarning($"CargoBox ({gameObject.name}): Slot {i} ve Slot {j} aynı pozisyonda ({productSlots[i].position})! Slot pozisyonlarını ayırın.");
                        }
                    }
                }
            }
        }
    }

    public void AssignOrder(OrderData order)
    {
        assignedOrder = order;
        Debug.Log($"Order assigned to CargoBox {gameObject.name}: {(order == null ? "NULL" : order.orderID)}");
    }

    public bool TryPlaceProduct(Product product)
    {
        if (product == null)
        {
            Debug.LogError($"CargoBox ({gameObject.name}): TryPlaceProduct çağrıldı ama product null!");
            return false;
        }
        if (productSlots == null || productSlots.Length == 0)
        {
            Debug.LogError($"CargoBox ({gameObject.name}): ProductSlots dizisi atanmamış veya boş!");
            return false;
        }

        if (!isLargeBox && product.productDefinition.isLarge)
        {
            Debug.LogWarning($"CargoBox ({gameObject.name}): Büyük ürün küçük kutuya yerleştirilemez! Ürün: {product.name}");
            return false;
        }

        // İlk boş slotu bul
        int slotIndex = -1;
        for (int i = 0; i < slotOccupied.Length; i++)
        {
            if (!slotOccupied[i])
            {
                slotIndex = i;
                break;
            }
        }

        if (slotIndex == -1)
        {
            Debug.LogWarning($"CargoBox ({gameObject.name}): Kutu dolu. {product.name} yerleştirilemedi.");
            return false;
        }

        Transform slot = productSlots[slotIndex];
        if (slot == null)
        {
            Debug.LogError($"CargoBox ({gameObject.name}): ProductSlots[{slotIndex}] null! {product.name} yerleştirilemiyor.");
            return false;
        }

        try
        {
            Debug.Log($"[CargoBox-TryPlace] {product.name} kutuya ({gameObject.name}) yerleştiriliyor. Slot: {slot.name}, Slot Pozisyonu: {slot.position}");

            // Rigidbody ayarları
            Rigidbody productRb = product.GetComponent<Rigidbody>();
            if (productRb != null)
            {
                productRb.isKinematic = true;
                productRb.velocity = Vector3.zero;
                productRb.angularVelocity = Vector3.zero;
            }

            // Collider ayarları
            Collider productCollider = product.GetComponent<Collider>();
            if (productCollider != null && boxCollider != null)
            {
                Physics.IgnoreCollision(boxCollider, productCollider, true);
                productCollider.enabled = true;
            }

            // Ölçek ayarları
            Vector3 originalWorldScale = product.GetOriginalWorldScale();
            if (originalWorldScale == Vector3.zero)
            {
                Debug.LogWarning($"CargoBox ({gameObject.name}): {product.name} için GetOriginalWorldScale() sıfır döndü. Varsayılan ölçek (1,1,1) kullanılacak.");
                originalWorldScale = Vector3.one;
            }

            Vector3 parentWorldScale = slot.lossyScale;
            parentWorldScale.x = Mathf.Approximately(parentWorldScale.x, 0) ? 1e-5f : parentWorldScale.x;
            parentWorldScale.y = Mathf.Approximately(parentWorldScale.y, 0) ? 1e-5f : parentWorldScale.y;
            parentWorldScale.z = Mathf.Approximately(parentWorldScale.z, 0) ? 1e-5f : parentWorldScale.z;

            // Ürünü slota yerleştir
            product.transform.SetParent(slot);
            product.transform.localPosition = Vector3.zero;
            product.transform.localRotation = Quaternion.identity;
            product.transform.localScale = new Vector3(
                originalWorldScale.x / parentWorldScale.x,
                originalWorldScale.y / parentWorldScale.y,
                originalWorldScale.z / parentWorldScale.z
            );

            // Görünürlük ayarları
            product.gameObject.SetActive(true);
            Renderer productRenderer = product.GetComponent<Renderer>();
            if (productRenderer != null)
                productRenderer.enabled = true;
            else
                Debug.LogWarning($"Product {product.name} üzerinde Renderer component'i bulunamadı!");

            // Ek görünürlük kontrolü
            if (!product.gameObject.activeSelf || (productRenderer != null && !productRenderer.enabled))
            {
                Debug.LogError($"Ürün {product.name} görünür değil! Active: {product.gameObject.activeSelf}, Renderer Enabled: {(productRenderer != null ? productRenderer.enabled : false)}");
            }

            // Ürünü listeye ekle ve slotu işgal et
            placedProducts.Add(product);
            slotOccupied[slotIndex] = true;

            Debug.Log($"<color=lime>[CargoBox-Placed]</color> {product.gameObject.name} -> Kutu: {gameObject.name}\n" +
                      $"Pozisyon: {product.transform.position}, Görünür: {product.gameObject.activeSelf}\n" +
                      $"Hedef Dünya Ölçek: {originalWorldScale}\n" +
                      $"Sonuç Dünya Ölçek: {product.transform.lossyScale}");
            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"TryPlaceProduct sırasında hata: {e.Message}");
            return false;
        }
    }

    public Product TryRemoveProduct(Vector3 rayOrigin, Vector3 rayDirection, float maxDistance)
    {
        if (isBeingCarried)
        {
            Debug.Log($"CargoBox ({gameObject.name}): Kutu taşınıyor, ürün alınamaz.");
            return null;
        }

        try
        {
            LayerMask pickupLayer = LayerMask.GetMask("Pickup");
            Ray ray = new Ray(rayOrigin, rayDirection);
            RaycastHit[] allHits = Physics.RaycastAll(ray, maxDistance, pickupLayer);

            Product productToTake = null;
            float minHitDistance = float.MaxValue;
            int slotIndexToFree = -1;

            Debug.Log($"[CargoBox-TryRemove] Raycast hits: {allHits.Length}, MaxDistance: {maxDistance}");
            foreach (RaycastHit hit in allHits)
            {
                Product hitProduct = hit.collider.GetComponent<Product>();
                if (hitProduct != null && placedProducts.Contains(hitProduct) && hit.collider.transform.IsChildOf(transform))
                {
                    if (hit.distance < minHitDistance)
                    {
                        minHitDistance = hit.distance;
                        productToTake = hitProduct;
                        // Slot indeksini bul
                        for (int i = 0; i < productSlots.Length; i++)
                        {
                            if (productSlots[i] != null && hitProduct.transform.parent == productSlots[i])
                            {
                                slotIndexToFree = i;
                                break;
                            }
                        }
                        Debug.Log($"Potential product to remove: {hitProduct.name}, Distance: {hit.distance}, SlotIndex: {slotIndexToFree}");
                    }
                }
            }

            if (productToTake != null)
            {
                placedProducts.Remove(productToTake);
                if (slotIndexToFree >= 0 && slotIndexToFree < slotOccupied.Length)
                {
                    slotOccupied[slotIndexToFree] = false;
                    Debug.Log($"Slot {slotIndexToFree} boşaltıldı.");
                }

                Vector3 originalWorldScale = productToTake.GetOriginalWorldScale();
                productToTake.transform.SetParent(null);
                productToTake.transform.localScale = originalWorldScale; // Ebeveyn yok, lokal ölçek = dünya ölçeği

                Collider productCollider = productToTake.GetComponent<Collider>();
                if (productCollider != null && boxCollider != null)
                {
                    Physics.IgnoreCollision(boxCollider, productCollider, false);
                }

                Rigidbody productRb = productToTake.GetComponent<Rigidbody>();
                if (productRb != null)
                {
                    productRb.isKinematic = false;
                    productRb.velocity = Vector3.zero;
                    productRb.angularVelocity = Vector3.zero;
                }

                productToTake.gameObject.SetActive(true);
                Renderer productRenderer = productToTake.GetComponent<Renderer>();
                if (productRenderer != null)
                    productRenderer.enabled = true;
                else
                    Debug.LogWarning($"Product {productToTake.name} üzerinde Renderer component'i bulunamadı!");

                if (!productToTake.gameObject.activeSelf || (productRenderer != null && !productRenderer.enabled))
                {
                    Debug.LogError($"Ürün {productToTake.name} görünür değil! Active: {productToTake.gameObject.activeSelf}, Renderer Enabled: {(productRenderer != null ? productRenderer.enabled : false)}");
                }

                Debug.Log($"[CargoBox-Remove] {productToTake.gameObject.name} kutudan ({gameObject.name}) alındı. Pozisyon: {productToTake.transform.position}, Görünür: {productToTake.gameObject.activeSelf}, Ölçek: {productToTake.transform.localScale}");
                return productToTake;
            }
            Debug.Log($"[CargoBox-Remove] Kutu ({gameObject.name}) içinden alınacak ürün bulunamadı.");
            return null;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"TryRemoveProduct sırasında hata: {e.Message}");
            return null;
        }
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
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }

    public bool IsBeingCarried()
    {
        return isBeingCarried;
    }
}