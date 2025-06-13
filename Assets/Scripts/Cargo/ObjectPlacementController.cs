using UnityEngine;
using System.Collections.Generic;

public class ObjectPlacementController : MonoBehaviour
{
    [Header("Yerleþtirme Ayarlarý")]
    [SerializeField] private float placementDistance = 10f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Material ghostMaterial;

    private GameObject heldFurniture;
    private Material originalMaterial;
    private Camera mainCamera;
    private LayerMask movableFurnitureLayer;
    private List<Rigidbody> affectedProductRigidbodies = new List<Rigidbody>();

    void Awake()
    {
        mainCamera = Camera.main;
        this.enabled = false;
        movableFurnitureLayer = LayerMask.GetMask("MovableFurniture");
        groundLayer = LayerMask.GetMask("Default", "Ground");
    }

    void Update()
    {
        if (heldFurniture == null)
        {
            HandlePickup();
        }
        else
        {
            HandleMovementAndPlacement();
        }
    }

    private void HandlePickup()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = mainCamera.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f, 0));
            if (Physics.Raycast(ray, out RaycastHit hit, placementDistance, movableFurnitureLayer))
            {
                heldFurniture = hit.collider.gameObject;

                if (heldFurniture.TryGetComponent<Rigidbody>(out var rb)) rb.isKinematic = true;

                affectedProductRigidbodies.Clear();
                Product[] productsOnShelf = heldFurniture.GetComponentsInChildren<Product>();
                foreach (Product product in productsOnShelf)
                {
                    if (product.TryGetComponent<Rigidbody>(out Rigidbody productRb))
                    {
                        if (!productRb.isKinematic)
                        {
                            productRb.isKinematic = true;
                            affectedProductRigidbodies.Add(productRb);
                        }
                    }
                }

                if (heldFurniture.TryGetComponent<Renderer>(out var renderer) && ghostMaterial != null)
                {
                    originalMaterial = renderer.material;
                    renderer.material = ghostMaterial;
                }
            }
        }
    }

    private void HandleMovementAndPlacement()
    {
        Ray ray = mainCamera.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f, 0));
        if (Physics.Raycast(ray, out RaycastHit hit, 100f, groundLayer))
        {
            heldFurniture.transform.position = hit.point;
        }

        if (Input.GetKeyDown(KeyCode.Q)) heldFurniture.transform.Rotate(Vector3.up, -45f);
        if (Input.GetKeyDown(KeyCode.E)) heldFurniture.transform.Rotate(Vector3.up, 45f);

        if (Input.GetMouseButtonDown(0))
        {
            if (heldFurniture.TryGetComponent<Rigidbody>(out var rb)) rb.isKinematic = false;

            foreach (Rigidbody productRb in affectedProductRigidbodies)
            {
                productRb.isKinematic = false;
            }
            affectedProductRigidbodies.Clear();

            if (heldFurniture.TryGetComponent<Renderer>(out var renderer) && originalMaterial != null)
            {
                renderer.material = originalMaterial;
            }

            heldFurniture = null;
            originalMaterial = null;
        }
    }
}