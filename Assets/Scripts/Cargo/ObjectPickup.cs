using UnityEngine;
using System.Collections;

public class ObjectPickup : MonoBehaviour
{
    [Header("Tutma ve Mesafe Ayarları")]
    [SerializeField] private Transform holdPosition;
    [SerializeField] private Transform slipHoldPosition;
    [SerializeField] private float pickupDistance = 5.0f;
    [SerializeField] private float shelfPlaceDistance = 2.0f;
    [SerializeField] private float cargoPlaceDistance = 7.0f;

    [Header("Yumuşak Alma Animasyon Ayarları")]
    [Tooltip("Objenin ele gelme animasyonunun saniye cinsinden süresi.")]
    [SerializeField] private float pickupDuration = 0.25f;
    [Tooltip("Animasyonun başlangıç ve bitişini yavaşlatarak daha yumuşak bir his verir.")]
    [SerializeField] private bool useSmoothStep = true;

    [Header("Diğer Ayarlar")]
    [SerializeField] private Color highlightColor = new Color(0, 1, 0, 1f);
    [SerializeField] private GameObject crosshair;
    [SerializeField] private Transform[] shelfSlots;
    [SerializeField] private float dropForwardOffset = 1.5f;

    private GameObject heldObject;
    private GameObject highlightedObject;
    private Camera mainCamera;
    private Color originalColor;
    private bool isHighlighted;
    private GameObject shelf;
    private LayerMask pickupLayer;
    private LayerMask slipLayer;
    private LayerMask cargoBoxLayer;
    private LayerMask interactableLayers;
    private Coroutine pickupCoroutine;

    private void Awake()
    {
        mainCamera = Camera.main;
        if (mainCamera == null) Debug.LogError("Main Camera bulunamadı!", this);
        if (holdPosition == null) Debug.LogError("HoldPosition atanmamış!", this);
        if (slipHoldPosition == null) Debug.LogError("SlipHoldPosition atanmamış!", this);
        if (crosshair == null) Debug.LogError("Crosshair atanmamış!", this);
        if (shelfSlots == null || shelfSlots.Length == 0) Debug.LogError("ShelfSlots atanmamış!", this);
        shelf = GameObject.FindWithTag("Shelf");
        if (shelf == null) Debug.LogError("Shelf tag’lı obje bulunamadı!", this);

        pickupLayer = LayerMask.GetMask("Pickup");
        slipLayer = LayerMask.GetMask("Slip");
        cargoBoxLayer = LayerMask.GetMask("CargoBox");
        interactableLayers = pickupLayer | slipLayer | cargoBoxLayer;
    }

    private void Update()
    {
        HandleInput();
        HighlightObjectUnderMouse();
        UpdateCrosshairVisibility();
    }

    private void HandleInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (pickupCoroutine != null) return;
            if (heldObject == null)
            {
                Ray ray = mainCamera.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f, 0));
                if (Physics.Raycast(ray, out RaycastHit hit, pickupDistance, interactableLayers))
                {
                    CargoBoxProxy proxy = hit.collider.GetComponentInParent<CargoBoxProxy>();
                    if (proxy != null && proxy.RealCargoBox != null)
                    {
                        TryPickupFromCargoBox(proxy.RealCargoBox, ray);
                    }
                    else
                    {
                        TryPickupFromWorld(hit);
                    }
                }
            }
            else
            {
                if (IsNearCargoBox(out CargoBox targetCargoBox)) TryPlaceObjectInCargoBox(targetCargoBox);
                else if (IsNearShelf() && heldObject.TryGetComponent<Product>(out _)) TryPlaceObjectOnShelf();
                else DropObject();
            }
        }

        if (Input.GetMouseButtonDown(1))
        {
            if (heldObject != null || pickupCoroutine != null) return;
            Ray ray = mainCamera.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f, 0));
            if (Physics.Raycast(ray, out RaycastHit hit, pickupDistance, cargoBoxLayer))
            {
                CargoBoxProxy proxy = hit.collider.GetComponentInParent<CargoBoxProxy>();
                if (proxy != null && proxy.RealCargoBox != null)
                {
                    proxy.RealCargoBox.ToggleLids();
                }
            }
        }
    }

    private void DropObject()
    {
        if (heldObject == null) return;
        if (pickupCoroutine != null)
        {
            StopCoroutine(pickupCoroutine);
            pickupCoroutine = null;
        }

        Vector3 dropPosition = FindSafeDropPosition();

        heldObject.transform.SetParent(null);

        if (heldObject.TryGetComponent<Product>(out var product))
        {
            heldObject.transform.localScale = product.GetOriginalWorldScale();
            product.isHeld = false;
        }
        if (heldObject.TryGetComponent<Slip>(out var slip))
        {
            heldObject.transform.localScale = slip.GetOriginalScale();
            slip.OnDropped();
        }

        Rigidbody rb = heldObject.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        heldObject.transform.position = dropPosition;
        heldObject.transform.rotation = Quaternion.identity;
        heldObject = null;
    }

    private Vector3 FindSafeDropPosition()
    {
        if (heldObject == null) return transform.position;

        Collider heldObjectCollider = heldObject.GetComponent<Collider>();
        if (heldObjectCollider == null)
        {
            Debug.LogError($"{heldObject.name} üzerinde Collider yok! Güvenli bırakma başarısız olabilir.");
            return mainCamera.transform.position + mainCamera.transform.forward * dropForwardOffset;
        }

        Ray ray = mainCamera.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f, 0));
        RaycastHit hit;

        // Işın göndermeden önce objenin collider'ını geçici olarak devre dışı bırak
        heldObjectCollider.enabled = false;

        bool surfaceFound = Physics.Raycast(ray, out hit, pickupDistance * 2f);

        // Collider'ı hemen geri aç
        heldObjectCollider.enabled = true;

        if (surfaceFound)
        {
            // hit.normal, yüzeyin baktığı yönü verir (zemin için yukarıyı gösterir).
            // bounds.extents, collider'ın merkezinden dış kenarına olan mesafedir.
            // Bu hesaplama, pivot nerede olursa olsun, objenin altını yüzeye tam oturtur.
            return hit.point + hit.normal * heldObjectCollider.bounds.extents.y;
        }
        else
        {
            // Eğer hiçbir yüzey bulunamazsa, oyuncunun önüne bırak (fallback)
            return mainCamera.transform.position + mainCamera.transform.forward * dropForwardOffset;
        }
    }

    private void HighlightObjectUnderMouse()
    {
        if (mainCamera == null) return;
        Ray ray = mainCamera.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f, 0));
        GameObject objectToHighlight = null;
        if (Physics.Raycast(ray, out RaycastHit hit, pickupDistance, interactableLayers))
        {
            if (hit.collider.gameObject == heldObject) return;
            CargoBoxProxy proxy = hit.collider.GetComponentInParent<CargoBoxProxy>();
            if (proxy != null && proxy.RealCargoBox != null)
            {
                Product productInBox = proxy.RealCargoBox.GetProductAtRay(ray, pickupDistance);
                if (productInBox != null)
                {
                    objectToHighlight = productInBox.gameObject;
                }
            }
            else if (hit.collider.CompareTag("Pickup") || hit.collider.CompareTag("SlipTag"))
            {
                objectToHighlight = hit.collider.gameObject;
            }
        }
        if (highlightedObject != objectToHighlight)
        {
            if (highlightedObject != null) RemoveHighlight();
            highlightedObject = objectToHighlight;
            if (highlightedObject != null) ApplyHighlight(highlightedObject);
        }
    }

    private void UpdateCrosshairVisibility()
    {
        if (crosshair != null)
        {
            bool shouldBeActive = (heldObject == null && pickupCoroutine == null);
            if (crosshair.activeSelf != shouldBeActive)
            {
                crosshair.SetActive(shouldBeActive);
            }
        }
    }

    private void TryPickupFromCargoBox(CargoBox cargoBox, Ray ray)
    {
        Product product = cargoBox.TryRemoveProduct(ray.origin, ray.direction, pickupDistance);
        if (product != null)
        {
            heldObject = product.gameObject;
            StartPickupAnimation();
        }
    }

    private void TryPickupFromWorld(RaycastHit hit)
    {
        GameObject targetObject = hit.collider.gameObject;
        if (targetObject.CompareTag("Pickup") || targetObject.CompareTag("SlipTag"))
        {
            if (targetObject.TryGetComponent<Product>(out var product) && targetObject.transform.parent != null)
            {
                targetObject.transform.SetParent(null);
                targetObject.transform.localScale = product.GetOriginalWorldScale();
            }
            if (targetObject.TryGetComponent<Slip>(out var slip) && targetObject.transform.parent != null)
            {
                targetObject.transform.SetParent(null);
                targetObject.transform.localScale = slip.GetOriginalScale();
            }
            heldObject = targetObject;
            if (highlightedObject == heldObject) RemoveHighlight();
            StartPickupAnimation();
        }
    }

    private void StartPickupAnimation()
    {
        if (heldObject == null) return;
        if (pickupCoroutine != null) StopCoroutine(pickupCoroutine);
        if (heldObject.TryGetComponent<Rigidbody>(out Rigidbody rb)) rb.isKinematic = true;
        Transform targetHoldPosition = heldObject.CompareTag("SlipTag") ? slipHoldPosition : holdPosition;
        pickupCoroutine = StartCoroutine(SmoothPickupCoroutine(heldObject.transform, targetHoldPosition));
        if (heldObject.TryGetComponent<Product>(out Product product)) product.OnPickedUp();
        if (heldObject.TryGetComponent<Slip>(out Slip slip)) slip.OnPickedUp();
    }

    private IEnumerator SmoothPickupCoroutine(Transform objectToMove, Transform targetHoldPosition)
    {
        float elapsedTime = 0f;
        Vector3 startPosition = objectToMove.position;
        Quaternion startRotation = objectToMove.rotation;
        while (elapsedTime < pickupDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsedTime / pickupDuration);
            if (useSmoothStep) progress = Mathf.SmoothStep(0, 1, progress);
            objectToMove.position = Vector3.Lerp(startPosition, targetHoldPosition.position, progress);
            objectToMove.rotation = Quaternion.Slerp(startRotation, targetHoldPosition.rotation, progress);
            yield return null;
        }
        objectToMove.SetParent(targetHoldPosition);
        objectToMove.position = targetHoldPosition.position;
        objectToMove.rotation = targetHoldPosition.rotation;
        if (objectToMove.CompareTag("Pickup"))
        {
            objectToMove.localPosition = Vector3.zero;
            objectToMove.localRotation = Quaternion.identity;
        }
        else if (objectToMove.CompareTag("SlipTag"))
        {
            objectToMove.localPosition = Vector3.zero;
            objectToMove.localRotation = Quaternion.Euler(-90f, 0f, 0f);
        }
        pickupCoroutine = null;
    }

    private void TryPlaceObjectOnShelf()
    {
        if (heldObject == null) return;
        Product productToPlace = heldObject.GetComponent<Product>();
        if (productToPlace == null) { DropObject(); return; }
        foreach (Transform slot in shelfSlots)
        {
            if (slot.childCount == 0)
            {
                heldObject.transform.SetParent(slot);
                heldObject.transform.localPosition = Vector3.zero;
                heldObject.transform.localRotation = Quaternion.identity;
                Vector3 parentWorldScale = slot.lossyScale;
                Vector3 originalWorldScale = productToPlace.GetOriginalWorldScale();
                heldObject.transform.localScale = new Vector3(
                    originalWorldScale.x / (parentWorldScale.x == 0 ? 1 : parentWorldScale.x),
                    originalWorldScale.y / (parentWorldScale.y == 0 ? 1 : parentWorldScale.y),
                    originalWorldScale.z / (parentWorldScale.z == 0 ? 1 : parentWorldScale.z)
                );
                if (heldObject.TryGetComponent<Rigidbody>(out Rigidbody rb)) rb.isKinematic = true;
                productToPlace.isHeld = false;
                heldObject = null;
                return;
            }
        }
        DropObject();
    }

    private void TryPlaceObjectInCargoBox(CargoBox cargoBox)
    {
        if (heldObject == null || cargoBox == null) return;
        Product product = heldObject.GetComponent<Product>();
        if (product != null && cargoBox.TryPlaceProduct(product))
        {
            heldObject = null;
        }
        else
        {
            DropObject();
        }
    }

    private void ApplyHighlight(GameObject obj)
    {
        if (obj == null) return;
        if (obj.TryGetComponent<Slip>(out Slip slip)) slip.Highlight(true);
        else if (obj.TryGetComponent<Renderer>(out Renderer renderer))
        {
            originalColor = renderer.material.color;
            renderer.material.color = highlightColor;
            isHighlighted = true;
        }
    }

    private void RemoveHighlight()
    {
        if (highlightedObject == null) return;
        if (highlightedObject.TryGetComponent<Slip>(out Slip slip)) slip.Highlight(false);
        else if (highlightedObject.TryGetComponent<Renderer>(out Renderer renderer) && isHighlighted)
            renderer.material.color = originalColor;
        isHighlighted = false;
    }

    private bool IsNearShelf() => shelf != null && Vector3.Distance(mainCamera.transform.position, shelf.transform.position) <= shelfPlaceDistance;

    private bool IsNearCargoBox(out CargoBox cargoBox)
    {
        cargoBox = null;
        Collider[] hits = Physics.OverlapSphere(mainCamera.transform.position, cargoPlaceDistance, cargoBoxLayer);
        foreach (Collider hit in hits)
        {
            CargoBoxProxy proxy = hit.GetComponentInParent<CargoBoxProxy>();
            if (proxy != null && proxy.RealCargoBox != null)
            {
                CargoBox cb = proxy.RealCargoBox;
                if (!cb.IsFull() && !cb.IsBeingCarried())
                {
                    cargoBox = cb;
                    return true;
                }
            }
        }
        return false;
    }

    public GameObject GetHeldObject() => heldObject;
    public void ClearHeldObject()
    {
        if (heldObject != null) Destroy(heldObject);
        heldObject = null;
    }
}