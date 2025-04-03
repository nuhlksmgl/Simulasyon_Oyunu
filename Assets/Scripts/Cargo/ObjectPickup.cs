using UnityEngine;

public class ObjectPickup : MonoBehaviour
{
    [SerializeField] private Transform holdPosition; // Nesnenin tutulacağı pozisyon
    [SerializeField] private float pickupDistance = 5.0f; // Alım mesafesi
    [SerializeField] private float lerpSpeed = 20f; // Hareket yumuşaklığı
    [SerializeField] private Color highlightColor = new Color(0, 1, 0, 1f); // Yeşil vurgu rengi
    [SerializeField] private GameObject crosshair; // Nişangah UI objesi
    [SerializeField] private Transform[] shelfSlots; // Raf slotlarının dizisi
    [SerializeField] private float shelfPlaceDistance = 2.0f; // Rafe yerleştirme mesafesi
    [SerializeField] private float cargoPlaceDistance = 2.0f; // Kargo kutusuna yerleştirme mesafesi

    private GameObject heldObject; // Tutulan nesne
    private GameObject highlightedObject; // Vurgulanan nesne
    private Camera mainCamera;
    private Color originalColor; // Nesnenin orijinal rengi
    private bool isHighlighted; // Vurgu durumunu takip et
    private GameObject shelf; // Raf objesi (referans için)
    private LayerMask pickupLayer; // Sadece Pickup layer’ını hedefleyen maske

    private void Awake()
    {
        mainCamera = Camera.main;
        if (holdPosition == null) Debug.LogError("HoldPosition atanmamış!", this);
        if (crosshair == null) Debug.LogError("Crosshair atanmamış!", this);
        if (shelfSlots == null || shelfSlots.Length == 0) Debug.LogError("ShelfSlots atanmamış!", this);
        shelf = GameObject.FindWithTag("Shelf");
        if (shelf == null) Debug.LogError("Shelf tag’lı obje bulunamadı!", this);
        pickupLayer = LayerMask.GetMask("Pickup");

        // HoldPosition’ın ölçeğini (1,1,1) yap
        if (holdPosition.localScale != Vector3.one)
        {
            Debug.LogWarning("HoldPosition’ın ölçeği (1,1,1) değil! Ölçek sıfırlanıyor.");
            holdPosition.localScale = Vector3.one;
        }
    }

    private void Update()
    {
        // HoldPosition’ın ölçeğini her frame’de kontrol et
        if (holdPosition.localScale != Vector3.one)
        {
            Debug.LogWarning($"HoldPosition’ın ölçeği değişti! Şu anki ölçek: {holdPosition.localScale}. Ölçek sıfırlanıyor.");
            holdPosition.localScale = Vector3.one;
        }

        HandleInput();
        UpdateHeldObjectPosition();
        HighlightObjectUnderMouse();
        UpdateCrosshairVisibility();
    }

    private void HandleInput()
    {
        if (Input.GetMouseButtonDown(0)) // Sol tık
        {
            if (heldObject == null)
            {
                // El boşsa yerden veya kargo kutusundan obje al
                TryPickupObject();
                if (heldObject == null) TryPickupFromCargoBox();
            }
            else
            {
                // Elde obje varsa rafa veya kargo kutusuna yerleştir
                if (IsNearShelf())
                {
                    TryPlaceObjectOnShelf();
                }
                else if (IsNearCargoBox(out CargoBox cargoBox))
                {
                    TryPlaceObjectInCargoBox(cargoBox);
                }
                else
                {
                    DropObject();
                }
            }
        }
    }

    private void UpdateHeldObjectPosition()
    {
        if (heldObject == null || holdPosition == null) return;

        // Pozisyonu her zaman güncelle
        heldObject.transform.position = Vector3.Lerp(
            heldObject.transform.position,
            holdPosition.position,
            Time.deltaTime * lerpSpeed);

        // Eğer tutulan nesne bir CargoBox ise rotasyonu her frame’de kameraya göre güncelle
        if (heldObject.TryGetComponent(out CargoBox cargoBox))
        {
            // Kameranın yönünü al (sadece yatay yön, Y eksenini sıfırla)
            Vector3 cameraForward = mainCamera.transform.forward;
            cameraForward.y = 0; // Y eksenini sıfırla (sadece yatay yönü al)
            float cameraYaw = Quaternion.LookRotation(cameraForward).eulerAngles.y;
            // CargoBox’ın rotasyonunu ayarla: X eksenini -90 derece sabit tut, Y ekseni kameraya göre güncellensin, Z ekseni 0
            heldObject.transform.rotation = Quaternion.Euler(-90, cameraYaw, 0);
        }
        else
        {
            // Diğer nesneler (örneğin Product) için rotasyonu holdPosition’a göre güncelle
            heldObject.transform.rotation = Quaternion.Lerp(
                heldObject.transform.rotation,
                holdPosition.rotation,
                Time.deltaTime * lerpSpeed);
        }
    }

    private void UpdateCrosshairVisibility()
    {
        if (crosshair != null)
        {
            crosshair.SetActive(heldObject == null);
        }
    }

    private void HighlightObjectUnderMouse()
    {
        if (mainCamera == null) return;

        Vector3 screenCenter = new Vector3(Screen.width / 2f, Screen.height / 2f, 0);
        Ray ray = mainCamera.ScreenPointToRay(screenCenter);
        bool hitSomething = Physics.Raycast(ray, out RaycastHit hit, pickupDistance, pickupLayer);

        GameObject newHighlightedObject = null;
        if (hitSomething)
        {
            GameObject hitObject = hit.collider.gameObject;
            if (hitObject.CompareTag("Pickup") && hitObject != heldObject)
            {
                newHighlightedObject = hitObject;
            }
            else if (hitObject.CompareTag("CargoBox") && hitObject != heldObject)
            {
                CargoBox cargoBox = hitObject.GetComponent<CargoBox>();
                if (cargoBox != null && !cargoBox.IsBeingCarried())
                {
                    newHighlightedObject = hitObject;
                }
            }
        }

        if (newHighlightedObject != highlightedObject)
        {
            if (highlightedObject != null && newHighlightedObject == null)
            {
                RemoveHighlight();
            }
            else if (newHighlightedObject != null && !isHighlighted)
            {
                highlightedObject = newHighlightedObject;
                ApplyHighlight(highlightedObject);
            }
        }
    }

    private void ApplyHighlight(GameObject obj)
    {
        if (obj == null) return;

        Renderer renderer = obj.GetComponent<Renderer>();
        if (renderer == null) return;

        originalColor = renderer.material.color;
        renderer.material.color = highlightColor;
        isHighlighted = true;
    }

    private void RemoveHighlight()
    {
        if (highlightedObject == null) return;

        Renderer renderer = highlightedObject.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = originalColor;
        }
        highlightedObject = null;
        isHighlighted = false;
    }

    private void TryPickupObject()
    {
        if (mainCamera == null) return;

        Vector3 screenCenter = new Vector3(Screen.width / 2f, Screen.height / 2f, 0);
        Ray ray = mainCamera.ScreenPointToRay(screenCenter);
        if (Physics.Raycast(ray, out RaycastHit hit, pickupDistance, pickupLayer))
        {
            GameObject targetObject = hit.collider.gameObject;
            if (targetObject.CompareTag("Pickup"))
            {
                heldObject = targetObject;
                RemoveHighlight();
                SetupHeldObject(heldObject);
                Debug.Log($"Alındı: {heldObject.name}");
            }
            else if (targetObject.CompareTag("CargoBox"))
            {
                CargoBox cargoBox = targetObject.GetComponent<CargoBox>();
                if (cargoBox != null && !cargoBox.IsBeingCarried())
                {
                    heldObject = targetObject;
                    RemoveHighlight();
                    SetupHeldObject(heldObject);
                    cargoBox.OnPickedUp();
                    Debug.Log($"CargoBox alındı: {heldObject.name}");
                }
            }
        }
    }

    private void TryPickupFromCargoBox()
    {
        if (mainCamera == null) return;

        Vector3 screenCenter = new Vector3(Screen.width / 2f, Screen.height / 2f, 0);
        Ray ray = mainCamera.ScreenPointToRay(screenCenter);
        Collider[] hits = Physics.OverlapSphere(mainCamera.transform.position, pickupDistance);
        foreach (Collider hit in hits)
        {
            if (hit.CompareTag("CargoBox"))
            {
                CargoBox cargoBox = hit.GetComponent<CargoBox>();
                if (cargoBox != null)
                {
                    Product product = cargoBox.TryRemoveProduct(ray.origin, ray.direction, pickupDistance);
                    if (product != null)
                    {
                        heldObject = product.gameObject;
                        SetupHeldObject(heldObject);
                        Debug.Log($"Kargo kutusundan alındı: {heldObject.name}");
                        break;
                    }
                }
            }
        }
    }

    private void SetupHeldObject(GameObject obj)
    {
        if (obj == null || holdPosition == null) return;

        // Product nesnesi ise ölçeği al
        Vector3 originalScale = obj.transform.localScale;
        if (obj.TryGetComponent(out Product product))
        {
            originalScale = product.GetOriginalScale(); // Orijinal ölçeği Product’tan al
            Debug.Log($"{obj.name} alındığında ölçek: {originalScale}");
        }

        obj.transform.SetParent(holdPosition);
        obj.transform.localPosition = Vector3.zero;

        // Ölçeği orijinal değerine geri getir
        obj.transform.localScale = originalScale;

        if (obj.TryGetComponent(out Rigidbody rb))
        {
            rb.isKinematic = true;
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        if (obj.TryGetComponent(out Product productComponent))
        {
            productComponent.OnPickedUp();
        }
    }

    private void DropObject()
    {
        if (heldObject == null) return;

        // Bırakmadan önce yere yakın bir pozisyon bul
        Vector3 dropPosition = heldObject.transform.position;
        if (Physics.Raycast(heldObject.transform.position, Vector3.down, out RaycastHit hit, 10f))
        {
            dropPosition = hit.point + Vector3.up * 0.5f; // Yerden biraz yukarıda bırak
        }

        Rigidbody rb = heldObject.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = false;
        }

        // Product nesnesi ise ölçeği al
        Vector3 originalScale = heldObject.transform.localScale;
        if (heldObject.TryGetComponent(out Product product))
        {
            originalScale = product.GetOriginalScale(); // Orijinal ölçeği Product’tan al
            Debug.Log($"{heldObject.name} bırakılmadan önce ölçek: {originalScale}");
        }

        heldObject.transform.SetParent(null);
        heldObject.transform.position = dropPosition; // Yere yakın bir pozisyona bırak
        heldObject.transform.localScale = originalScale; // Ölçeği koru

        // CargoBox bırakıldığında rotasyonu kameraya göre ayarla
        if (heldObject.TryGetComponent(out CargoBox cargoBox))
        {
            Vector3 cameraForward = mainCamera.transform.forward;
            cameraForward.y = 0;
            float cameraYaw = Quaternion.LookRotation(cameraForward).eulerAngles.y;
            heldObject.transform.rotation = Quaternion.Euler(-90, cameraYaw, 0); // X eksenini -90 olarak koru
        }

        Debug.Log($"{heldObject.name} bırakıldı. Pozisyon: {heldObject.transform.position}, Ölçek: {heldObject.transform.localScale}");

        if (heldObject.TryGetComponent(out Product productComponent))
        {
            productComponent.isHeld = false;
            productComponent.ResetPosition();
            Debug.Log($"{heldObject.name} ResetPosition sonrası pozisyon: {heldObject.transform.position}, Ölçek: {heldObject.transform.localScale}");
        }
        else if (heldObject.TryGetComponent(out CargoBox cargoBox2))
        {
            cargoBox2.OnDropped();
        }

        heldObject = null;
    }

    private void TryPlaceObjectOnShelf()
    {
        if (heldObject == null || shelf == null) return;

        for (int i = 0; i < shelfSlots.Length; i++)
        {
            if (shelfSlots[i] != null && !IsSlotOccupied(shelfSlots[i]))
            {
                // Product nesnesi ise ölçeği al
                Vector3 originalScale = heldObject.transform.localScale;
                if (heldObject.TryGetComponent(out Product product))
                {
                    originalScale = product.GetOriginalScale(); // Orijinal ölçeği Product’tan al
                }

                heldObject.transform.SetParent(shelf.transform);
                heldObject.transform.position = shelfSlots[i].position;
                heldObject.transform.rotation = shelfSlots[i].rotation;

                // Shelf’in ölçeğine göre Product’ın localScale’ini ayarla
                Vector3 shelfScale = shelf.transform.localScale;
                heldObject.transform.localScale = new Vector3(
                    originalScale.x / shelfScale.x,
                    originalScale.y / shelfScale.y,
                    originalScale.z / shelfScale.z
                );

                if (heldObject.TryGetComponent(out Rigidbody rb))
                {
                    rb.velocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                    rb.isKinematic = true;
                }

                if (heldObject.TryGetComponent(out Product productComponent))
                {
                    productComponent.isHeld = false;
                }
                else if (heldObject.TryGetComponent(out CargoBox cargoBox))
                {
                    cargoBox.OnDropped();
                }

                Debug.Log($"{heldObject.name} raf slotuna yerleştirildi: {shelfSlots[i].name}, Pozisyon: {heldObject.transform.position}, Local Ölçek: {heldObject.transform.localScale}, Dünya Ölçeği: {heldObject.transform.lossyScale}");
                heldObject = null;
                return;
            }
        }
        Debug.Log("Raf dolu, yerleştirme yapılamadı!");
    }

    private void TryPlaceObjectInCargoBox(CargoBox cargoBox)
    {
        if (heldObject == null || cargoBox == null) return;

        // Product nesnesi ise ölçeği al
        Vector3 originalScale = heldObject.transform.localScale;
        if (heldObject.TryGetComponent(out Product product))
        {
            originalScale = product.GetOriginalScale(); // Orijinal ölçeği Product’tan al
            if (cargoBox.TryPlaceProduct(product))
            {
                heldObject.transform.localScale = originalScale; // Ölçeği koru
                heldObject = null; // El boşaltılır
            }
        }
    }

    private bool IsSlotOccupied(Transform slot)
    {
        Collider[] colliders = Physics.OverlapSphere(slot.position, 0.1f, pickupLayer);
        foreach (Collider col in colliders)
        {
            if (col.CompareTag("Pickup") && col.gameObject != heldObject)
            {
                return true;
            }
        }
        return false;
    }

    private bool IsNearShelf()
    {
        if (shelf == null) return false;

        float distanceToShelf = Vector3.Distance(mainCamera.transform.position, shelf.transform.position);
        return distanceToShelf <= shelfPlaceDistance;
    }

    private bool IsNearCargoBox(out CargoBox cargoBox)
    {
        cargoBox = null;
        Collider[] hits = Physics.OverlapSphere(mainCamera.transform.position, cargoPlaceDistance);
        foreach (Collider hit in hits)
        {
            if (hit.CompareTag("CargoBox"))
            {
                cargoBox = hit.GetComponent<CargoBox>();
                if (cargoBox != null && !cargoBox.IsFull() && !cargoBox.IsBeingCarried())
                {
                    return true;
                }
            }
        }
        return false;
    }

    public GameObject GetHeldObject()
    {
        return heldObject;
    }

    public void ClearHeldObject()
    {
        heldObject = null;
    }
}