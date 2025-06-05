using UnityEngine;
using System.Collections; // Coroutine için (şimdilik kullanılmıyor ama gelecekte gerekebilir)

public class ObjectPickup : MonoBehaviour
{
    [SerializeField] private Transform holdPosition;
    [SerializeField] private Transform slipHoldPosition;
    [SerializeField] private float pickupDistance = 5.0f;
    [SerializeField] private float lerpSpeed = 20f;
    [SerializeField] private Color highlightColor = new Color(0, 1, 0, 1f);
    [SerializeField] private GameObject crosshair;
    [SerializeField] private Transform[] shelfSlots;
    [SerializeField] private float shelfPlaceDistance = 2.0f;
    [SerializeField] private float cargoPlaceDistance = 7.0f;
    [SerializeField] private LayerMask groundLayerMask;
    [SerializeField] private float dropForwardOffset = 2.0f; // Bu, basit testte kullanılmayacak ama kalsın

    private GameObject heldObject;
    private GameObject highlightedObject;
    private Camera mainCamera;
    private Color originalColor;
    private bool isHighlighted;
    private GameObject shelf;
    private LayerMask pickupLayer;
    private LayerMask slipLayer;
    private bool isProcessingInput = false;
    private Transform currentHoldPosition;

    private Vector3 originalWorldScale;
    private Quaternion originalLocalRotation;

    private void Awake()
    {
        mainCamera = Camera.main;
        if (mainCamera == null) Debug.LogError("Main Camera bulunamadı!", this);

        if (holdPosition == null) Debug.LogError("HoldPosition atanmamış!", this);
        else
        {
            if (holdPosition.localScale != Vector3.one)
            {
                Debug.LogWarning("HoldPosition’ın ölçeği (1,1,1) değil! Ölçek (1,1,1) olarak sıfırlanıyor.");
                holdPosition.localScale = Vector3.one;
            }
        }

        if (slipHoldPosition == null) Debug.LogError("SlipHoldPosition atanmamış! Ölçek ayarlanamıyor.", this);
        else
        {
            slipHoldPosition.localScale = new Vector3(2f, 2f, 2f);
        }

        if (crosshair == null) Debug.LogError("Crosshair atanmamış!", this);
        if (shelfSlots == null || shelfSlots.Length == 0) Debug.LogError("ShelfSlots atanmamış!", this);

        shelf = GameObject.FindWithTag("Shelf");
        if (shelf == null) Debug.LogError("Shelf tag’lı obje bulunamadı!", this);

        pickupLayer = LayerMask.GetMask("Pickup");
        slipLayer = LayerMask.GetMask("Slip");

        if (groundLayerMask == 0)
        {
            Debug.LogWarning("Ground Layer Mask atanmamış! Physics.DefaultRaycastLayers kullanılacak, bu istenmeyen sonuçlara yol açabilir.");
        }
    }

    private void Update()
    {
        try
        {
            HandleInput();
            UpdateHeldObjectPosition();
            HighlightObjectUnderMouse();
            UpdateCrosshairVisibility();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Update sırasında hata: {e.Message}\n{e.StackTrace}");
        }
    }

    private void HandleInput()
    {
        try
        {
            if (isProcessingInput) return;
            if (Input.GetMouseButtonDown(0))
            {
                isProcessingInput = true;
                if (heldObject == null)
                {
                    TryPickupFromCargoBox();
                    if (heldObject == null)
                    {
                        TryPickupFromShelf();
                    }
                }
                else
                {
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
                isProcessingInput = false;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"HandleInput sırasında hata: {e.Message}\n{e.StackTrace}");
            isProcessingInput = false;
        }
    }

    private void SetupHeldObject()
    {
        if (heldObject == null) return;
        try
        {
            originalWorldScale = heldObject.transform.lossyScale;
            originalLocalRotation = heldObject.transform.localRotation;

            if (heldObject.CompareTag("SlipTag"))
            {
                currentHoldPosition = slipHoldPosition;
            }
            else
            {
                currentHoldPosition = holdPosition;
            }

            if (currentHoldPosition == null)
            {
                Debug.LogError($"Hata: currentHoldPosition null! Obje: {heldObject.name}, Tag: {heldObject.tag}");
                if (this.heldObject != null) DropObject();
                return;
            }

            heldObject.transform.SetParent(currentHoldPosition);
            heldObject.transform.localPosition = Vector3.zero;

            if (heldObject.CompareTag("SlipTag"))
            {
                heldObject.transform.localRotation = Quaternion.Euler(-90f, 0f, 0f);
            }
            else
            {
                heldObject.transform.localRotation = Quaternion.identity;
            }

            heldObject.transform.localScale = originalWorldScale;

            if (heldObject.TryGetComponent(out Rigidbody rb))
            {
                rb.isKinematic = true;
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

            if (heldObject.TryGetComponent(out Product product)) product.OnPickedUp();
            else if (heldObject.TryGetComponent<Slip>(out Slip slip) && heldObject.CompareTag("SlipTag"))
            {
                // Slip.OnPickedUp(), TryPickupFromShelf içinde çağrılıyor.
            }

            heldObject.SetActive(true);
            Renderer renderer = heldObject.GetComponent<Renderer>();
            if (renderer != null) renderer.enabled = true;

            if (!heldObject.activeSelf || (renderer != null && !renderer.enabled))
                Debug.LogError($"HeldObject {heldObject.name} görünür değil! Active: {heldObject.activeSelf}, Renderer Enabled: {(renderer != null ? renderer.enabled.ToString() : "N/A")}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"SetupHeldObject sırasında hata ({heldObject?.name}): {e.Message}\n{e.StackTrace}");
        }
    }

    private void UpdateHeldObjectPosition()
    {
        if (heldObject == null || currentHoldPosition == null) return;
        try
        {
            Transform targetParent = null;
            if (heldObject.CompareTag("SlipTag"))
            {
                if (currentHoldPosition != slipHoldPosition) targetParent = slipHoldPosition;
            }
            else if (heldObject.CompareTag("Pickup"))
            {
                if (currentHoldPosition != holdPosition) targetParent = holdPosition;
            }

            if (targetParent != null)
            {
                currentHoldPosition = targetParent;
                if (heldObject.transform.parent != currentHoldPosition)
                {
                    heldObject.transform.SetParent(currentHoldPosition);
                }
            }

            heldObject.transform.position = Vector3.Lerp(heldObject.transform.position, currentHoldPosition.position, Time.deltaTime * lerpSpeed);

            if (heldObject.CompareTag("SlipTag"))
            {
                heldObject.transform.localRotation = Quaternion.Euler(-90f, 0f, 0f);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"UpdateHeldObjectPosition sırasında hata ({heldObject?.name}): {e.Message}\n{e.StackTrace}");
        }
    }

    private void UpdateCrosshairVisibility()
    {
        try
        {
            if (crosshair != null) crosshair.SetActive(heldObject == null);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"UpdateCrosshairVisibility sırasında hata: {e.Message}\n{e.StackTrace}");
        }
    }

    private void HighlightObjectUnderMouse()
    {
        if (mainCamera == null) return;
        try
        {
            Vector3 screenCenter = new Vector3(Screen.width / 2f, Screen.height / 2f, 0);
            Ray ray = mainCamera.ScreenPointToRay(screenCenter);

            bool hitSlip = Physics.Raycast(ray, out RaycastHit slipHit, pickupDistance, slipLayer);
            bool hitPickup = Physics.Raycast(ray, out RaycastHit pickupHit, pickupDistance, pickupLayer);

            GameObject objectFoundByRay = null;
            if (hitSlip)
            {
                GameObject hitObject = slipHit.collider.gameObject;
                if (hitObject.CompareTag("SlipTag") && hitObject != heldObject)
                {
                    objectFoundByRay = hitObject;
                }
            }

            if (objectFoundByRay == null && hitPickup)
            {
                GameObject hitObject = pickupHit.collider.gameObject;
                if (hitObject.CompareTag("Pickup") && hitObject != heldObject)
                {
                    objectFoundByRay = hitObject;
                }
            }

            if (highlightedObject != objectFoundByRay)
            {
                if (highlightedObject != null)
                {
                    RemoveHighlight();
                }
                highlightedObject = objectFoundByRay;
                if (highlightedObject != null)
                {
                    ApplyHighlight(highlightedObject);
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"HighlightObjectUnderMouse sırasında hata: {e.Message}\n{e.StackTrace}");
        }
    }

    private void ApplyHighlight(GameObject objToHighlight)
    {
        if (objToHighlight == null) return;
        try
        {
            if (objToHighlight.TryGetComponent<Slip>(out Slip slipComponent))
            {
                slipComponent.Highlight(true);
            }
            else if (objToHighlight.CompareTag("Pickup") && objToHighlight.TryGetComponent<Renderer>(out Renderer renderer))
            {
                if (objToHighlight.TryGetComponent<Product>(out _))
                {
                    originalColor = renderer.material.color;
                    renderer.material.color = highlightColor;
                    isHighlighted = true;
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"ApplyHighlight sırasında hata ({objToHighlight?.name}): {e.Message}\n{e.StackTrace}");
        }
    }

    private void RemoveHighlight()
    {
        if (highlightedObject == null) return;
        try
        {
            if (highlightedObject.TryGetComponent<Slip>(out Slip slipComponent))
            {
                slipComponent.Highlight(false);
            }
            else if (highlightedObject.CompareTag("Pickup") && highlightedObject.TryGetComponent<Renderer>(out Renderer renderer))
            {
                if (highlightedObject.TryGetComponent<Product>(out _))
                {
                    if (isHighlighted)
                    {
                        renderer.material.color = originalColor;
                    }
                }
            }
            isHighlighted = false;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"RemoveHighlight sırasında hata ({highlightedObject?.name}): {e.Message}\n{e.StackTrace}");
        }
    }

    private void TryPickupFromCargoBox()
    {
        if (mainCamera == null || heldObject != null) return;
        try
        {
            Vector3 screenCenter = new Vector3(Screen.width / 2f, Screen.height / 2f, 0);
            Ray ray = mainCamera.ScreenPointToRay(screenCenter);
            Collider[] hits = Physics.OverlapSphere(mainCamera.transform.position, cargoPlaceDistance, LayerMask.GetMask("CargoBox"));
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
                            SetupHeldObject();
                            return;
                        }
                    }
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"TryPickupFromCargoBox sırasında hata: {e.Message}\n{e.StackTrace}");
        }
    }

    private void TryPickupFromShelf()
    {
        if (mainCamera == null || heldObject != null) return;
        try
        {
            Vector3 screenCenter = new Vector3(Screen.width / 2f, Screen.height / 2f, 0);
            Ray ray = mainCamera.ScreenPointToRay(screenCenter);
            RaycastHit hitInfo;

            if (Physics.Raycast(ray, out hitInfo, pickupDistance, slipLayer))
            {
                GameObject targetObject = hitInfo.collider.gameObject;
                if (targetObject.CompareTag("SlipTag"))
                {
                    heldObject = targetObject;
                    if (highlightedObject == heldObject) RemoveHighlight();
                    SetupHeldObject();
                    if (heldObject.TryGetComponent<Slip>(out Slip slipComponent))
                    {
                        slipComponent.OnPickedUp();
                    }
                    return;
                }
            }

            if (Physics.Raycast(ray, out hitInfo, pickupDistance, pickupLayer))
            {
                GameObject targetObject = hitInfo.collider.gameObject;
                if (targetObject.CompareTag("Pickup"))
                {
                    heldObject = targetObject;
                    if (highlightedObject == heldObject) RemoveHighlight();
                    SetupHeldObject();
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"TryPickupFromShelf sırasında hata: {e.Message}\n{e.StackTrace}");
        }
    }

    private void DropObject()
    {
        if (heldObject == null) return;

        GameObject objectToDrop = heldObject;

        try
        {
            // --- PRODUCT'LAR İÇİN GEÇİCİ BASİT BIRAKMA TESTİ ---
            if (objectToDrop.CompareTag("Pickup")) // Product ise
            {
                Debug.LogWarning($"[DropObject_ProductTest] {objectToDrop.name} BASİT BIRAKMA mantığı ile bırakılıyor.");
                objectToDrop.transform.SetParent(null);

                // Oyuncunun (bu script'in bağlı olduğu transform) biraz önüne ve biraz yukarısına bırak
                Vector3 testDropPos = transform.position + transform.forward * 2.0f + transform.up * 0.5f;
                objectToDrop.transform.position = testDropPos;
                objectToDrop.transform.rotation = Quaternion.identity; // Dünya rotasyonunu sıfırla
                objectToDrop.transform.localScale = originalWorldScale; // Orijinal dünya ölçeğini geri yükle

                if (objectToDrop.TryGetComponent(out Rigidbody rbP)) // 'rbP' Product için
                {
                    rbP.isKinematic = false;
                    rbP.velocity = Vector3.zero;
                    rbP.angularVelocity = Vector3.zero;
                    // Product prefab'ının Collision Detection ayarını kullanması için burada zorlamıyoruz,
                    // ama test için ContinuousSpeculative yapabilirsiniz:
                    // rbP.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
                    Debug.Log($"[DropObject_ProductTest] {objectToDrop.name} Rigidbody: isKinematic={rbP.isKinematic}, collisionMode={rbP.collisionDetectionMode}, pos={objectToDrop.transform.position}");
                }

                if (objectToDrop.TryGetComponent<Product>(out Product productComponent))
                {
                    if (productComponent.isHeld) productComponent.isHeld = false;
                }
            }
            // --- SLIP'LER VE DİĞERLERİ İÇİN NORMAL BIRAKMA MANTIĞI ---
            else
            {
                Vector3 dropDirectionXZ = mainCamera.transform.forward;
                dropDirectionXZ.y = 0;
                if (dropDirectionXZ.sqrMagnitude < 0.001f)
                {
                    dropDirectionXZ = transform.forward;
                    dropDirectionXZ.y = 0;
                    if (dropDirectionXZ.sqrMagnitude < 0.001f)
                    {
                        dropDirectionXZ = transform.forward;
                        if (dropDirectionXZ.sqrMagnitude < 0.001f) dropDirectionXZ = Vector3.forward;
                    }
                }
                dropDirectionXZ.Normalize();

                Vector3 horizontalDropBase = transform.position + dropDirectionXZ * dropForwardOffset;
                Vector3 rayStartPoint = new Vector3(horizontalDropBase.x, transform.position.y + 1.0f, horizontalDropBase.z);

                float objectHeightOffset = objectToDrop.transform.lossyScale.y / 2f;
                objectHeightOffset = Mathf.Max(0.01f, objectHeightOffset);

                Vector3 finalDropPosition;
                bool groundFound = false;
                RaycastHit groundHit;
                float raycastDistance = 10f;

                if (Physics.Raycast(rayStartPoint, Vector3.down, out groundHit, raycastDistance, groundLayerMask))
                {
                    finalDropPosition = groundHit.point + (Vector3.up * (objectHeightOffset + 0.02f));
                    groundFound = true;
                }
                else
                {
                    finalDropPosition = new Vector3(horizontalDropBase.x,
                                                    transform.position.y + objectHeightOffset,
                                                    horizontalDropBase.z);
                    Debug.LogWarning($"[DropObject] {objectToDrop.name} için zemin bulunamadı. Fallback pozisyonu: {finalDropPosition}");
                }

                // Debug.Log($"[DropObject] {objectToDrop.name} - Nihai Bırakma Pozisyonu: {finalDropPosition}, Zemin Bulundu: {groundFound}, Çarpılan: {(groundFound ? groundHit.collider.name : "Yok")}");

                // Penetration Check (Slip veya diğerleri için de yapılabilir, şimdilik sadece Product için yukarıdaydı)
                // ...

                objectToDrop.transform.SetParent(null);
                objectToDrop.transform.position = finalDropPosition;
                objectToDrop.transform.localRotation = originalLocalRotation;
                objectToDrop.transform.localScale = originalWorldScale;

                if (objectToDrop.TryGetComponent(out Rigidbody rb))
                {
                    rb.isKinematic = false;
                    rb.velocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;

                    if (objectToDrop.CompareTag("SlipTag"))
                    {
                        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
                    }
                }

                if (objectToDrop.TryGetComponent<Product>(out Product productComponent)) // Bu blok artık yukarıdaki özel Product bloğunda
                {
                    // if (productComponent.isHeld) productComponent.isHeld = false; 
                }
                if (objectToDrop.TryGetComponent<Slip>(out Slip slipComponent))
                {
                    slipComponent.OnDropped();
                }
            }

            // Her iki dal için ortak:
            objectToDrop.SetActive(true);
            Renderer renderer = objectToDrop.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.enabled = true;
            }

            if (this.heldObject == objectToDrop)
            {
                this.heldObject = null;
                this.currentHoldPosition = null;
            }

            if (highlightedObject == objectToDrop)
            {
                highlightedObject = null;
                isHighlighted = false;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[DropObject] sırasında HATA ({objectToDrop?.name}): {e.Message}\n{e.StackTrace}");
            if (objectToDrop != null)
            {
                Debug.LogWarning($"[DropObject] Hata sonrası {objectToDrop.name} için acil durum bırakma.");
                if (!objectToDrop.activeSelf) objectToDrop.SetActive(true);
                Renderer rend = objectToDrop.GetComponent<Renderer>();
                if (rend != null && !rend.enabled) rend.enabled = true;
                else if (rend == null) Debug.LogWarning($"[DropObject] Catch: {objectToDrop.name} için Renderer bulunamadı.");
                if (objectToDrop.transform.parent != null) objectToDrop.transform.SetParent(null);
                if (objectToDrop.TryGetComponent(out Rigidbody rbCatch))
                {
                    rbCatch.isKinematic = false;
                    if (objectToDrop.CompareTag("SlipTag")) rbCatch.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
                }
                if (this.heldObject == objectToDrop) { this.heldObject = null; this.currentHoldPosition = null; }
            }
            else if (this.heldObject != null)
            {
                Debug.LogWarning($"[DropObject] Hata sonrası (objectToDrop null): {this.heldObject.name} zorla bırakılıyor.");
                GameObject emergencyDropObj = this.heldObject;
                if (!emergencyDropObj.activeSelf) emergencyDropObj.SetActive(true);
                Renderer emergencyRend = emergencyDropObj.GetComponent<Renderer>();
                if (emergencyRend != null && !emergencyRend.enabled) emergencyRend.enabled = true;
                emergencyDropObj.transform.SetParent(null);
                if (emergencyDropObj.TryGetComponent(out Rigidbody rbE))
                {
                    rbE.isKinematic = false;
                    if (emergencyDropObj.CompareTag("SlipTag")) rbE.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
                }
                this.heldObject = null; this.currentHoldPosition = null;
            }
        }
    }

    private bool IsSlotOccupied(Transform slot)
    {
        if (slot == null) return true;
        try
        {
            Collider[] colliders = Physics.OverlapSphere(slot.position, 0.1f, pickupLayer | slipLayer);
            foreach (Collider col in colliders)
            {
                if (col.gameObject != heldObject && (col.CompareTag("Pickup") || col.CompareTag("SlipTag")))
                {
                    return true;
                }
            }
            return false;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"IsSlotOccupied sırasında hata ({slot?.name}): {e.Message}\n{e.StackTrace}");
            return true;
        }
    }

    private void TryPlaceObjectOnShelf()
    {
        if (heldObject == null || shelf == null || shelfSlots == null || shelfSlots.Length == 0)
        {
            if (heldObject != null) DropObject();
            return;
        }
        try
        {
            if (heldObject.CompareTag("SlipTag") || heldObject.TryGetComponent<Slip>(out _))
            {
                DropObject();
                return;
            }

            if (!heldObject.TryGetComponent<Product>(out _))
            {
                DropObject();
                return;
            }

            for (int i = 0; i < shelfSlots.Length; i++)
            {
                if (shelfSlots[i] != null && !IsSlotOccupied(shelfSlots[i]))
                {
                    Transform targetSlot = shelfSlots[i];
                    Vector3 parentWorldScale = targetSlot.lossyScale;
                    parentWorldScale.x = Mathf.Approximately(parentWorldScale.x, 0) ? 1e-5f : parentWorldScale.x;
                    parentWorldScale.y = Mathf.Approximately(parentWorldScale.y, 0) ? 1e-5f : parentWorldScale.y;
                    parentWorldScale.z = Mathf.Approximately(parentWorldScale.z, 0) ? 1e-5f : parentWorldScale.z;

                    heldObject.transform.SetParent(targetSlot);
                    heldObject.transform.localPosition = Vector3.zero;
                    heldObject.transform.localRotation = Quaternion.identity;
                    heldObject.transform.localScale = new Vector3(
                        originalWorldScale.x / parentWorldScale.x,
                        originalWorldScale.y / parentWorldScale.y,
                        originalWorldScale.z / parentWorldScale.z
                    );

                    if (heldObject.TryGetComponent(out Rigidbody rb))
                    {
                        rb.isKinematic = true;
                        rb.velocity = Vector3.zero;
                        rb.angularVelocity = Vector3.zero;
                    }

                    if (heldObject.TryGetComponent<Product>(out Product product)) product.isHeld = false;

                    heldObject.SetActive(true);
                    Renderer renderer = heldObject.GetComponent<Renderer>();
                    if (renderer != null) renderer.enabled = true;

                    heldObject = null;
                    currentHoldPosition = null;
                    return;
                }
            }
            DropObject();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"TryPlaceObjectOnShelf sırasında hata ({heldObject?.name}): {e.Message}\n{e.StackTrace}");
            if (this.heldObject != null) DropObject();
        }
    }

    private void TryPlaceObjectInCargoBox(CargoBox cargoBox)
    {
        if (heldObject == null || cargoBox == null)
        {
            if (heldObject != null) DropObject();
            return;
        }
        try
        {
            Product product = heldObject.GetComponent<Product>();
            if (product == null)
            {
                DropObject();
                return;
            }
            if (cargoBox.TryPlaceProduct(product))
            {
                heldObject = null;
                currentHoldPosition = null;
            }
            else
            {
                DropObject();
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"TryPlaceObjectInCargoBox sırasında hata ({cargoBox?.name}, {heldObject?.name}): {e.Message}\n{e.StackTrace}");
            if (this.heldObject != null) DropObject();
        }
    }

    private bool IsNearShelf()
    {
        if (shelf == null) return false;
        try
        {
            float distanceToShelf = Vector3.Distance(mainCamera.transform.position, shelf.transform.position);
            return distanceToShelf <= shelfPlaceDistance;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"IsNearShelf sırasında hata: {e.Message}\n{e.StackTrace}");
            return false;
        }
    }

    private bool IsNearCargoBox(out CargoBox cargoBox)
    {
        cargoBox = null;
        try
        {
            Collider[] hits = Physics.OverlapSphere(mainCamera.transform.position, cargoPlaceDistance, LayerMask.GetMask("CargoBox"));
            foreach (Collider hit in hits)
            {
                if (hit.CompareTag("CargoBox"))
                {
                    CargoBox cb = hit.GetComponent<CargoBox>();
                    if (cb != null && !cb.IsFull() && !cb.IsBeingCarried())
                    {
                        cargoBox = cb;
                        return true;
                    }
                }
            }
            return false;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"IsNearCargoBox sırasında hata: {e.Message}\n{e.StackTrace}");
            return false;
        }
    }

    public GameObject GetHeldObject() => heldObject;

    public void ClearHeldObject()
    {
        if (heldObject != null)
        {
            heldObject.transform.SetParent(null);
            if (heldObject.TryGetComponent(out Rigidbody rb) && rb.isKinematic)
            {
                rb.isKinematic = false;
                if (heldObject.CompareTag("SlipTag"))
                {
                    // rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
                }
            }
        }
        heldObject = null;
        currentHoldPosition = null;
    }
}