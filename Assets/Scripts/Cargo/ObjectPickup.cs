using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class ObjectPickup : MonoBehaviour
{
    [Header("Tutma ve Mesafe Ayarları")]
    [SerializeField] private Transform holdPosition;
    [SerializeField] private Transform slipHoldPosition;
    [SerializeField] private float pickupDistance = 5.0f;
    [SerializeField] private float shelfPlaceDistance = 2.0f;
    [SerializeField] private float cargoPlaceDistance = 7.0f;
    [Tooltip("Slip'i yakındaki bir kutuya yapıştırmak için maksimum mesafe.")]
    [SerializeField] private float attachmentRadius = 2.5f;

    [Header("Büyük Nesne Taşıma Ayarları")]
    [SerializeField] private Transform containerHoldPosition;
    [SerializeField] private float carryDistance = 4.0f;

    [Header("Yumuşak Alma Animasyon Ayarları")]
    [SerializeField] private float pickupDuration = 0.25f;
    [SerializeField] private bool useSmoothStep = true;

    [Header("Diğer Ayarlar")]
    [SerializeField] private Color highlightColor = new Color(0, 1, 0, 1f);
    [SerializeField] private GameObject crosshair;
    [SerializeField] private List<Transform> initialShelfSlots;
    [SerializeField] private float dropForwardOffset = 1.5f;

    private List<Transform> allAvailableSlots = new List<Transform>();
    private GameObject heldObject;
    private GameObject carriedContainer;
    private GameObject highlightedObject;
    private Camera mainCamera;
    private Color originalColor;
    private bool isHighlighted;
    private LayerMask pickupLayer, slipLayer, cargoBoxLayer, interactableLayers;
    private Coroutine pickupCoroutine, containerPickupCoroutine;
    private Vector3 originalContainerScale;

    private void Awake()
    {
        mainCamera = Camera.main;
        allAvailableSlots.AddRange(initialShelfSlots);
        pickupLayer = LayerMask.GetMask("Pickup");
        slipLayer = LayerMask.GetMask("Slip");
        cargoBoxLayer = LayerMask.GetMask("CargoBox");
        interactableLayers = pickupLayer | slipLayer | cargoBoxLayer;
    }

    private void Update()
    {
        HandlePickupAndPlaceInput();
        HandleInteractionInput();
        HighlightObjectUnderMouse();
        UpdateCrosshairVisibility();
    }

    private void HandleInteractionInput()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            if (heldObject != null && heldObject.TryGetComponent<Slip>(out Slip heldSlip))
            {
                Collider[] nearbyColliders = Physics.OverlapSphere(transform.position, attachmentRadius, cargoBoxLayer);

                List<CargoBox> validTargets = new List<CargoBox>();
                foreach (var boxCollider in nearbyColliders)
                {
                    if (boxCollider.GetComponentInParent<CargoBox>() is CargoBox currentBox)
                    {
                        if (!currentBox.HasValidAssignedOrder())
                        {
                            validTargets.Add(currentBox);
                        }
                    }
                }

                if (validTargets.Count == 0)
                {
                    return;
                }

                CargoBox closestBox = validTargets.OrderBy(box => Vector3.Distance(transform.position, box.transform.position)).First();

                if (closestBox != null)
                {
                    closestBox.AttachSlip(heldSlip);
                    heldObject = null;
                }
            }
        }
    }

    private void HandlePickupAndPlaceInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (heldObject != null)
            {
                Ray placementRay = mainCamera.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f, 0));
                float placeDistance = Mathf.Max(shelfPlaceDistance, cargoPlaceDistance);
                if (Physics.Raycast(placementRay, out RaycastHit placementHit, placeDistance))
                {
                    if (placementHit.collider.CompareTag("Shelf")) { TryPlaceObjectOnShelf(); }
                    else if (placementHit.collider.GetComponentInParent<CargoBox>() is CargoBox box) { TryPlaceObjectInCargoBox(box); }
                    else { DropObject(); }
                }
                else { DropObject(); }
                return;
            }

            if (carriedContainer != null)
            {
                DropCarriedContainer();
                return;
            }

            if (pickupCoroutine != null || containerPickupCoroutine != null) return;

            Ray ray = mainCamera.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f, 0));
            float maxDistance = Mathf.Max(pickupDistance, carryDistance);
            if (Physics.Raycast(ray, out RaycastHit hit, maxDistance, interactableLayers))
            {
                if (hit.collider.GetComponentInParent<CargoBox>() is CargoBox box)
                {
                    if (box.IsOpen && box.GetProductAtRay(ray, pickupDistance) != null)
                    {
                        TryPickupFromCargoBox(box, ray);
                    }
                    else if (Vector3.Distance(transform.position, hit.point) <= carryDistance)
                    {
                        carriedContainer = box.gameObject;
                        originalContainerScale = carriedContainer.transform.localScale;
                        box.OnPickedUp();
                        containerPickupCoroutine = StartCoroutine(SmoothContainerPickupCoroutine(carriedContainer.transform, containerHoldPosition));
                    }
                }
                else if (hit.collider.CompareTag("Pickup") || hit.collider.CompareTag("SlipTag"))
                {
                    TryPickupFromWorld(hit);
                }
            }
        }

        if (Input.GetMouseButtonDown(1))
        {
            if (IsHoldingAnything() || pickupCoroutine != null || containerPickupCoroutine != null) return;
            Ray ray = mainCamera.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f, 0));
            if (Physics.Raycast(ray, out RaycastHit hit, pickupDistance, cargoBoxLayer))
            {
                if (hit.collider.GetComponentInParent<CargoBox>() is CargoBox box)
                {
                    box.ToggleLids();
                }
            }
        }
    }

    public void RegisterSlots(Transform[] newSlots)
    {
        allAvailableSlots.AddRange(newSlots);
    }

    public void UnregisterSlots(Transform[] slotsToRemove)
    {
        foreach (var slot in slotsToRemove)
        {
            allAvailableSlots.Remove(slot);
        }
    }

    public bool IsHoldingAnything()
    {
        return heldObject != null || carriedContainer != null;
    }

    private void DropCarriedContainer()
    {
        if (carriedContainer == null) return;
        if (containerPickupCoroutine != null)
        {
            StopCoroutine(containerPickupCoroutine);
            containerPickupCoroutine = null;
        }
        carriedContainer.transform.SetParent(null);
        carriedContainer.transform.localScale = originalContainerScale;
        if (carriedContainer.TryGetComponent<CargoBox>(out CargoBox box))
        {
            box.OnDropped();
        }
        carriedContainer = null;
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
        heldObject.transform.SetPositionAndRotation(dropPosition, Quaternion.identity);
        heldObject = null;
    }

    private Vector3 FindSafeDropPosition()
    {
        if (heldObject == null) return transform.position;
        Collider c = heldObject.GetComponent<Collider>();
        if (c == null) return mainCamera.transform.position + mainCamera.transform.forward * dropForwardOffset;
        Ray r = mainCamera.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f, 0));
        c.enabled = false;
        bool found = Physics.Raycast(r, out RaycastHit hit, pickupDistance * 2f);
        c.enabled = true;
        if (found) return hit.point + hit.normal * c.bounds.extents.y;
        return mainCamera.transform.position + mainCamera.transform.forward * dropForwardOffset;
    }

    private void HighlightObjectUnderMouse()
    {
        if (mainCamera == null) return;
        Ray ray = mainCamera.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f, 0));
        GameObject objectToHighlight = null;
        if (Physics.Raycast(ray, out RaycastHit hit, pickupDistance, interactableLayers))
        {
            if (heldObject != null && hit.collider.gameObject == heldObject) return;
            if (carriedContainer != null && hit.collider.transform.IsChildOf(carriedContainer.transform)) return;
            if (hit.collider.GetComponentInParent<CargoBox>() is CargoBox box)
            {
                Product productInBox = box.GetProductAtRay(ray, pickupDistance);
                if (productInBox != null) objectToHighlight = productInBox.gameObject;
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
            bool shouldBeActive = !IsHoldingAnything() && pickupCoroutine == null && containerPickupCoroutine == null;
            if (crosshair.activeSelf != shouldBeActive) crosshair.SetActive(shouldBeActive);
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
        if (targetObject.TryGetComponent<Product>(out var p) && p.transform.parent != null)
        {
            p.transform.SetParent(null);
            p.transform.localScale = p.GetOriginalWorldScale();
        }
        if (targetObject.TryGetComponent<Slip>(out var s) && s.transform.parent != null)
        {
            s.transform.SetParent(null);
            s.transform.localScale = s.GetOriginalScale();
        }
        heldObject = targetObject;
        if (highlightedObject == heldObject) RemoveHighlight();
        StartPickupAnimation();
    }

    private void StartPickupAnimation()
    {
        if (heldObject == null) return;
        if (pickupCoroutine != null) StopCoroutine(pickupCoroutine);
        if (heldObject.TryGetComponent<Rigidbody>(out Rigidbody rb)) rb.isKinematic = true;
        Transform target = heldObject.CompareTag("SlipTag") ? slipHoldPosition : holdPosition;
        pickupCoroutine = StartCoroutine(SmoothPickupCoroutine(heldObject.transform, target));
        if (heldObject.TryGetComponent<Product>(out Product p)) p.OnPickedUp();
        if (heldObject.TryGetComponent<Slip>(out Slip s)) s.OnPickedUp();
    }

    private IEnumerator SmoothPickupCoroutine(Transform obj, Transform target)
    {
        float e = 0f; Vector3 sP = obj.position; Quaternion sR = obj.rotation;
        while (e < pickupDuration)
        {
            e += Time.deltaTime; float p = Mathf.Clamp01(e / pickupDuration);
            if (useSmoothStep) p = Mathf.SmoothStep(0, 1, p);
            obj.SetPositionAndRotation(Vector3.Lerp(sP, target.position, p), Quaternion.Slerp(sR, target.rotation, p));
            yield return null;
        }
        obj.SetParent(target);
        obj.SetPositionAndRotation(target.position, target.rotation);
        obj.localPosition = Vector3.zero;
        if (obj.CompareTag("Pickup")) obj.localRotation = Quaternion.identity;
        else if (obj.CompareTag("SlipTag")) obj.localRotation = Quaternion.Euler(-90f, 0f, 0f);
        pickupCoroutine = null;
    }

    private IEnumerator SmoothContainerPickupCoroutine(Transform obj, Transform target)
    {
        float e = 0f; Vector3 sP = obj.position; Quaternion sR = obj.rotation;
        while (e < pickupDuration)
        {
            e += Time.deltaTime; float p = Mathf.Clamp01(e / pickupDuration);
            if (useSmoothStep) p = Mathf.SmoothStep(0, 1, p);
            obj.SetPositionAndRotation(Vector3.Lerp(sP, target.position, p), Quaternion.Slerp(sR, target.rotation, p));
            yield return null;
        }
        obj.SetParent(target);
        obj.SetPositionAndRotation(target.position, target.rotation);
        obj.localPosition = Vector3.zero;
        obj.localRotation = Quaternion.identity;
        containerPickupCoroutine = null;
    }

    private void TryPlaceObjectOnShelf()
    {
        if (heldObject == null) return;
        Product p = heldObject.GetComponent<Product>();
        if (p == null) { DropObject(); return; }
        Transform bestSlot = null; float closestDist = float.MaxValue;
        var emptySlots = allAvailableSlots.Where(s => s.childCount == 0).ToList();
        foreach (Transform s in emptySlots)
        {
            float d = Vector3.Distance(mainCamera.transform.position, s.position);
            if (d < closestDist) { closestDist = d; bestSlot = s; }
        }
        if (bestSlot != null && closestDist <= shelfPlaceDistance)
        {
            heldObject.transform.SetParent(bestSlot);
            heldObject.transform.localPosition = Vector3.zero; // Pozisyonu sıfırla
            heldObject.transform.localRotation = Quaternion.identity; // Rotasyonu sıfırla
            // Ürünün orijinal ölçeğini rafın hiyerarşisine göre doğru uygula
            Vector3 parentWorldScale = bestSlot.lossyScale;
            heldObject.transform.localScale = new Vector3(
                p.GetOriginalWorldScale().x / parentWorldScale.x,
                p.GetOriginalWorldScale().y / parentWorldScale.y,
                p.GetOriginalWorldScale().z / parentWorldScale.z
            );
            if (heldObject.TryGetComponent<Rigidbody>(out Rigidbody rb)) rb.isKinematic = true;
            p.isHeld = false;
            heldObject = null;
        }
        else DropObject();
    }

    private void TryPlaceObjectInCargoBox(CargoBox box)
    {
        if (heldObject == null || box == null) return;
        Product p = heldObject.GetComponent<Product>();
        if (p != null && box.TryPlaceProduct(p)) heldObject = null;
        else DropObject();
    }

    private void ApplyHighlight(GameObject obj)
    {
        if (obj == null) return;
        if (obj.TryGetComponent<Slip>(out Slip s)) s.Highlight(true);
        else if (obj.TryGetComponent<Renderer>(out Renderer r))
        {
            originalColor = r.material.color;
            r.material.color = highlightColor;
            isHighlighted = true;
        }
    }

    private void RemoveHighlight()
    {
        if (highlightedObject == null) return;
        if (highlightedObject.TryGetComponent<Slip>(out Slip s)) s.Highlight(false);
        else if (highlightedObject.TryGetComponent<Renderer>(out Renderer r) && isHighlighted) r.material.color = originalColor;
        isHighlighted = false;
    }

    public GameObject GetHeldObject() => heldObject;
    public void ClearHeldObject() { if (heldObject != null) Destroy(heldObject); heldObject = null; }
}