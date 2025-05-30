using UnityEngine;

public class ObjectPickup : MonoBehaviour
{
    [SerializeField] private Transform holdPosition;
    [SerializeField] private float pickupDistance = 5.0f;
    [SerializeField] private float lerpSpeed = 20f;
    [SerializeField] private Color highlightColor = new Color(0, 1, 0, 1f);
    [SerializeField] private GameObject crosshair;
    [SerializeField] private Transform[] shelfSlots;
    [SerializeField] private float shelfPlaceDistance = 2.0f;
    [SerializeField] private float cargoPlaceDistance = 2.0f;

    private GameObject heldObject;
    private GameObject highlightedObject;
    private Camera mainCamera;
    private Color originalColor;
    private bool isHighlighted;
    private GameObject shelf;
    private LayerMask pickupLayer;

    private void Awake()
    {
        mainCamera = Camera.main;
        if (holdPosition == null) Debug.LogError("HoldPosition atanmamış!", this);
        if (crosshair == null) Debug.LogError("Crosshair atanmamış!", this);
        if (shelfSlots == null || shelfSlots.Length == 0) Debug.LogError("ShelfSlots atanmamış!", this);
        shelf = GameObject.FindWithTag("Shelf");
        if (shelf == null) Debug.LogError("Shelf tag’lı obje bulunamadı!", this);
        pickupLayer = LayerMask.GetMask("Pickup", "CargoBox");

        if (holdPosition.localScale != Vector3.one)
        {
            Debug.LogWarning("HoldPosition’ın ölçeği (1,1,1) değil! Ölçek sıfırlanıyor.");
            holdPosition.localScale = Vector3.one;
        }
    }

    private void Update()
    {
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
        if (Input.GetMouseButtonDown(0))
        {
            if (heldObject == null)
            {
                TryPickupFromCargoBox();
                TryPickupFromShelf();
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
        }
    }

    private void UpdateHeldObjectPosition()
    {
        if (heldObject == null || holdPosition == null) return;

        heldObject.transform.position = Vector3.Lerp(
            heldObject.transform.position,
            holdPosition.position,
            Time.deltaTime * lerpSpeed);

        heldObject.transform.localRotation = Quaternion.identity;
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

    private void TryPickupFromCargoBox()
    {
        if (mainCamera == null) return;

        Vector3 screenCenter = new Vector3(Screen.width / 2f, Screen.height / 2f, 0);
        Ray ray = mainCamera.ScreenPointToRay(screenCenter);
        Collider[] hits = Physics.OverlapSphere(mainCamera.transform.position, cargoPlaceDistance);
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
                        Debug.Log($"Kargo kutusundan alındı: {heldObject.name}");
                        break;
                    }
                }
            }
        }
    }

    private void TryPickupFromShelf()
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
                SetupHeldObject();
                Debug.Log($"Alındı: {heldObject.name}");
            }
        }
    }

    private void SetupHeldObject()
    {
        if (heldObject == null || holdPosition == null) return;

        Vector3 originalScale = heldObject.transform.localScale;
        if (heldObject.TryGetComponent(out Product product))
        {
            originalScale = product.GetOriginalScale();
            Debug.Log($"{heldObject.name} alındığında ölçek: {originalScale}");
        }

        heldObject.transform.SetParent(holdPosition);
        heldObject.transform.localPosition = Vector3.zero;

        heldObject.transform.localScale = originalScale;

        if (heldObject.TryGetComponent(out Rigidbody rb))
        {
            rb.isKinematic = true;
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        if (heldObject.TryGetComponent(out Product productComponent))
        {
            productComponent.OnPickedUp();
        }

        heldObject.SetActive(true);
        Renderer renderer = heldObject.GetComponent<Renderer>();
        if (renderer != null)
            renderer.enabled = true;
    }

    private void DropObject()
    {
        if (heldObject == null) return;

        Vector3 dropPosition = heldObject.transform.position;
        if (Physics.Raycast(heldObject.transform.position, Vector3.down, out RaycastHit hit, 10f))
        {
            dropPosition = hit.point + Vector3.up * 0.5f;
        }

        Rigidbody rb = heldObject.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = false;
        }

        Vector3 originalScale = heldObject.transform.localScale;
        if (heldObject.TryGetComponent(out Product product))
        {
            originalScale = product.GetOriginalScale();
            Debug.Log($"{heldObject.name} bırakılmadan önce ölçek: {originalScale}");
        }

        heldObject.transform.SetParent(null);
        heldObject.transform.position = dropPosition;
        heldObject.transform.localScale = originalScale;

        Debug.Log($"{heldObject.name} bırakıldı. Pozisyon: {heldObject.transform.position}, Ölçek: {heldObject.transform.localScale}, active = {heldObject.activeSelf}");

        if (heldObject.TryGetComponent(out Product productComponent))
        {
            productComponent.isHeld = false;
            productComponent.ResetPosition();
            Debug.Log($"{heldObject.name} ResetPosition sonrası pozisyon: {heldObject.transform.position}, Ölçek: {heldObject.transform.localScale}");
        }

        heldObject.SetActive(true);
        Renderer renderer = heldObject.GetComponent<Renderer>();
        if (renderer != null)
            renderer.enabled = true;
        else
            Debug.LogWarning($"Product {heldObject.name} has no Renderer component!");

        heldObject = null;
    }

    private void TryPlaceObjectOnShelf()
    {
        if (heldObject == null || shelf == null) return;

        for (int i = 0; i < shelfSlots.Length; i++)
        {
            if (shelfSlots[i] != null && !IsSlotOccupied(shelfSlots[i]))
            {
                Vector3 originalScale = heldObject.transform.localScale;
                if (heldObject.TryGetComponent(out Product product))
                {
                    originalScale = product.GetOriginalScale();
                }

                heldObject.transform.SetParent(shelf.transform);
                heldObject.transform.position = shelfSlots[i].position;
                heldObject.transform.rotation = shelfSlots[i].rotation;

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

        if (heldObject.TryGetComponent(out Product product))
        {
            if (cargoBox.TryPlaceProduct(product))
            {
                heldObject = null;
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