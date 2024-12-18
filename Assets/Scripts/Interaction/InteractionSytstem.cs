using UnityEngine;
using TMPro;

public class InteractionSystem : MonoBehaviour
{
    [Header("Interaction Settings")]
    [SerializeField] private float interactionRange = 2f;
    [SerializeField] private LayerMask interactionLayer;
    [SerializeField] private Transform holdPosition;

    [Header("UI Elements")]
    [SerializeField] private GameObject promptUI;
    [SerializeField] private TextMeshProUGUI promptText;

    private Camera playerCamera;
    private InteractableObject currentInteractable;
    public Rigidbody heldObject { get; private set; }

    private void Start()
    {
        playerCamera = GetComponentInChildren<Camera>();
        promptUI?.SetActive(false);
    }

    private void Update()
    {
        if (heldObject == null)
        {
            CheckForInteractable();
        }

        HandleInteraction();
        UpdateHeldObject();
    }

    private void CheckForInteractable()
    {
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactionRange, interactionLayer))
        {
            InteractableObject interactable = hit.collider.GetComponent<InteractableObject>();

            if (interactable != null)
            {
                // Yeni bir interactable'a bakıyorsak
                if (currentInteractable != interactable)
                {
                    // Eski interactable'ı temizle
                    currentInteractable?.Highlight(false);

                    // Yeni interactable'ı ayarla
                    currentInteractable = interactable;
                    ShowPrompt(currentInteractable.GetPromptMessage());
                    currentInteractable.Highlight(true);
                }
            }
        }
        else
        {
            // Hiçbir şeye bakmıyoruz
            currentInteractable?.Highlight(false);
            currentInteractable = null;
            HidePrompt();
        }
    }

    private void HandleInteraction()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (heldObject != null) // Bir şey tutuyorsak
            {
                DropObject();
            }
            else if (currentInteractable != null) // Etkileşime girebileceğimiz bir şey varsa
            {
                if (currentInteractable.CanPickup)
                {
                    PickupObject();
                }
                else
                {
                    currentInteractable.OnInteract();
                }
            }
        }
    }

    private void PickupObject()
    {
        Rigidbody rb = currentInteractable.GetComponent<Rigidbody>();
        if (rb != null)
        {
            heldObject = rb;
            heldObject.useGravity = false;
            heldObject.drag = 10;
            heldObject.constraints = RigidbodyConstraints.FreezeRotation;
            currentInteractable.Highlight(false);

            // Eğer bir ürünse, OnPickedUp fonksiyonunu çağır
            Product product = heldObject.GetComponent<Product>();
            if (product != null)
            {
                product.OnPickedUp(); // Ürün alındığında yapılacak işlemler
            }

            ShowPrompt("Press E to drop");
        }
    }

    public void DropObject()
    {
        if (heldObject != null)
        {
            heldObject.useGravity = true;
            heldObject.drag = 1;
            heldObject.constraints = RigidbodyConstraints.None;

            // Eğer bir ürünse, OnDropped fonksiyonunu çağır
            Product product = heldObject.GetComponent<Product>();
            if (product != null)
            {
                product.isHeld = false; // Ürün bırakıldı
            }

            heldObject = null;
            HidePrompt();
        }
    }

    private void UpdateHeldObject()
    {
        if (heldObject != null)
        {
            Vector3 targetPosition = holdPosition.position;
            Vector3 direction = targetPosition - heldObject.position;
            heldObject.velocity = direction * 12f;
        }
    }

    private void ShowPrompt(string message)
    {
        if (promptUI != null && promptText != null)
        {
            promptUI.SetActive(true);
            promptText.text = message;
        }
    }

    private void HidePrompt()
    {
        if (promptUI != null)
        {
            promptUI.SetActive(false);
        }
    }
}
