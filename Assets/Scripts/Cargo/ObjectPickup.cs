using UnityEngine;
// using TMPro; // Bu script'te doğrudan TMPro kullanılmıyorsa bu satır gereksiz olabilir.
// using System.Text; // Bu script'te doğrudan StringBuilder vb. kullanılmıyorsa bu satır gereksiz olabilir.

public class ObjectPickup : MonoBehaviour
{
    [SerializeField] private Transform holdPosition;
    [SerializeField] private Transform slipHoldPosition;
    [SerializeField] private float pickupDistance = 5.0f;
    [SerializeField] private float lerpSpeed = 20f;
    [SerializeField] private Color highlightColor = new Color(0, 1, 0, 1f); // Product highlight için
    [SerializeField] private GameObject crosshair;
    [SerializeField] private Transform[] shelfSlots;
    [SerializeField] private float shelfPlaceDistance = 2.0f;
    [SerializeField] private float cargoPlaceDistance = 7.0f;

    private GameObject heldObject;
    private GameObject highlightedObject;
    private Camera mainCamera;
    private Color originalColor; // Product'ların orijinal rengini saklamak için
    private bool isHighlighted;  // Bir Product'ın vurgulanıp vurgulanmadığını belirtir
    private GameObject shelf;
    private LayerMask pickupLayer;
    private LayerMask slipLayer;
    private bool isProcessingInput = false;
    private Transform currentHoldPosition;

    private Vector3 originalWorldScale;
    private Quaternion originalLocalRotation; // Objelerin bırakıldığında döneceği orijinal lokal rotasyon

    private void Awake()
    {
        mainCamera = Camera.main;
        if (mainCamera == null) Debug.LogError("Main Camera bulunamadı!", this);

        if (holdPosition == null) Debug.LogError("HoldPosition atanmamış!", this);
        else
        {
            // Debug.Log($"HoldPosition başlangıç pozisyonu: {holdPosition.position}, ölçeği: {holdPosition.localScale}");
            if (holdPosition.localScale != Vector3.one)
            {
                Debug.LogWarning("HoldPosition’ın ölçeği (1,1,1) değil! Ölçek (1,1,1) olarak sıfırlanıyor.");
                holdPosition.localScale = Vector3.one;
            }
        }

        // --- SLIPHOLDPOSITION İÇİN ÖLÇEK AYARI ---
        if (slipHoldPosition == null) Debug.LogError("SlipHoldPosition atanmamış! Ölçek ayarlanamıyor.", this);
        else
        {
            // Debug.Log($"SlipHoldPosition başlangıç pozisyonu: {slipHoldPosition.position}, mevcut ölçeği: {slipHoldPosition.localScale}. Ölçek (2,2,2) olarak ayarlanıyor.");
            slipHoldPosition.localScale = new Vector3(2f, 2f, 2f); // İstenen ölçek
        }
        // --- ÖLÇEK AYARI SONU ---

        if (crosshair == null) Debug.LogError("Crosshair atanmamış!", this);
        if (shelfSlots == null || shelfSlots.Length == 0) Debug.LogError("ShelfSlots atanmamış!", this);

        shelf = GameObject.FindWithTag("Shelf");
        if (shelf == null) Debug.LogError("Shelf tag’lı obje bulunamadı!", this);

        pickupLayer = LayerMask.GetMask("Pickup");
        slipLayer = LayerMask.GetMask("Slip");
    }

    private void Update()
    {
        try
        {
            // holdPosition'ın ölçeğinin runtime'da değişip değişmediğini kontrol et (isteğe bağlı)
            if (holdPosition != null && holdPosition.localScale != Vector3.one)
            {
                Debug.LogWarning($"HoldPosition’ın ölçeği Update'te değişti! Şu anki ölçek: {holdPosition.localScale}. Ölçek (1,1,1)'e sıfırlanıyor.");
                holdPosition.localScale = Vector3.one;
            }

            // slipHoldPosition'ın ölçeği Awake'te (2,2,2) olarak ayarlandı.
            // Update'te sürekli kontrol edip sıfırlayan kod kaldırıldı.
            // Eğer runtime'da başka bir şeyin bu ölçeği değiştirmemesini garantilemek isterseniz,
            // buraya bir kontrol ve düzeltme eklenebilir:
            // if (slipHoldPosition != null && slipHoldPosition.localScale != new Vector3(2f, 2f, 2f))
            // {
            //     Debug.LogWarning($"SlipHoldPosition’ın ölçeği Update'te (2,2,2)'den farklı! Şu anki ölçek: {slipHoldPosition.localScale}. Ölçek (2,2,2)'ye sıfırlanıyor.");
            //     slipHoldPosition.localScale = new Vector3(2f, 2f, 2f);
            // }

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
                if (this.heldObject != null) Debug.LogWarning($"Acil durum: {this.heldObject.name} objesi bırakılıyor çünkü currentHoldPosition null.");
                DropObject();
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

            // ÖNEMLİ: Bu satır, parent'in (currentHoldPosition) ölçeği 1 değilse,
            // tutulan objenin efektif dünya boyutunu değiştirir.
            // slipHoldPosition.localScale (2,2,2) ise, slip tutulurken 2 kat büyük görünür.
            heldObject.transform.localScale = originalWorldScale;

            if (heldObject.TryGetComponent(out Rigidbody rb))
            {
                rb.isKinematic = true;
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

            if (heldObject.TryGetComponent(out Product product)) product.OnPickedUp();
            else if (heldObject.TryGetComponent(out Slip slip) && heldObject.CompareTag("SlipTag"))
            {
                // Slip.OnPickedUp() zaten TryPickupFromShelf içinde çağrılıyor.
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
            if (heldObject.CompareTag("SlipTag") && currentHoldPosition != slipHoldPosition)
            {
                currentHoldPosition = slipHoldPosition;
                if (heldObject.transform.parent != currentHoldPosition)
                {
                    heldObject.transform.SetParent(currentHoldPosition);
                    // Parent değiştiğinde localPosition'ı tekrar sıfırlamak iyi bir fikir olabilir
                    // Eğer pozisyonda ani sıçramalar oluyorsa:
                    // heldObject.transform.localPosition = Vector3.zero;
                }
            }
            else if (!heldObject.CompareTag("SlipTag") && currentHoldPosition != holdPosition)
            {
                if (heldObject.CompareTag("Pickup"))
                {
                    currentHoldPosition = holdPosition;
                    if (heldObject.transform.parent != currentHoldPosition)
                    {
                        heldObject.transform.SetParent(currentHoldPosition);
                        // heldObject.transform.localPosition = Vector3.zero;
                    }
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
        try
        {
            Vector3 dropPositionBase = mainCamera.transform.position + mainCamera.transform.forward * 1.5f;
            float objectHeightOffset = heldObject.transform.lossyScale.y / 2f;

            if (Physics.Raycast(dropPositionBase + Vector3.up * 5f, Vector3.down, out RaycastHit groundHit, 10f))
            {
                dropPositionBase.y = groundHit.point.y + objectHeightOffset;
            }
            else
            {
                dropPositionBase.y = Mathf.Max(0.05f + objectHeightOffset, transform.position.y - 1f + objectHeightOffset);
            }

            heldObject.transform.SetParent(null);
            heldObject.transform.position = dropPositionBase;
            heldObject.transform.localRotation = originalLocalRotation;
            heldObject.transform.localScale = originalWorldScale;

            if (heldObject.TryGetComponent(out Rigidbody rb))
            {
                rb.isKinematic = false;
            }

            if (heldObject.TryGetComponent<Product>(out Product product))
            {
                if (product.isHeld) product.isHeld = false;
            }
            if (heldObject.TryGetComponent<Slip>(out Slip slip))
            {
                slip.OnDropped();
            }

            heldObject.SetActive(true);
            Renderer renderer = heldObject.GetComponent<Renderer>();
            if (renderer != null) renderer.enabled = true;

            GameObject tempHeldObject = heldObject;
            heldObject = null;
            currentHoldPosition = null;

            if (highlightedObject == tempHeldObject)
            {
                highlightedObject = null;
                isHighlighted = false;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"DropObject sırasında hata ({heldObject?.name}): {e.Message}\n{e.StackTrace}");
            if (this.heldObject != null)
            {
                Debug.LogWarning($"DropObject hatası sonrası {this.heldObject.name} zorla null yapılıyor.");
                this.heldObject.transform.SetParent(null);
                if (this.heldObject.TryGetComponent(out Rigidbody rb)) rb.isKinematic = false;
                this.heldObject = null;
                this.currentHoldPosition = null;
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
            }
        }
        heldObject = null;
        currentHoldPosition = null;
    }
}