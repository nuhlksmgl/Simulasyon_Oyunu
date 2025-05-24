using UnityEngine;
using System.Collections.Generic;

public class CargoBox : MonoBehaviour
{
    public bool isLargeBox;
    public float detectionRadius = 3f;
    public Transform[] productSlots;

    private List<Product> placedProducts = new List<Product>();
    public OrderData assignedOrder;

    private Rigidbody rb;
    private Collider boxCollider;
    private bool isBeingCarried = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        boxCollider = GetComponent<Collider>();
    }

    public void AssignOrder(OrderData order)
    {
        assignedOrder = order;
    }

    public void PlaceInitialProducts(OrderData order)
    {
        foreach (var item in order.itemsInOrder)
        {
            for (int i = 0; i < item.quantity; i++)
            {
                GameObject prefab = item.productDefinition.prefab;
                if (prefab == null) continue;

                GameObject productObj = Instantiate(prefab);
                Product product = productObj.GetComponent<Product>();

                if (product != null)
                {
                    product.productDefinition = item.productDefinition;
                    TryPlaceProduct(product);
                }
            }
        }
    }

    public bool IsInRange(Vector3 productPosition)
    {
        return Vector3.Distance(transform.position, productPosition) <= detectionRadius;
    }

    public bool TryPlaceProduct(Product product)
    {
        // Eğer sipariş atanmışsa, sadece o siparişin ürününü al
        if (assignedOrder != null)
        {
            bool isValid = false;
            foreach (var item in assignedOrder.itemsInOrder)
            {
                if (item.productDefinition.productName == product.productDefinition.productName)
                {
                    isValid = true;
                    break;
                }
            }

            if (!isValid)
                return false;
        }

        if (placedProducts.Count >= productSlots.Length)
            return false;

        int slotIndex = placedProducts.Count;
        Transform slot = productSlots[slotIndex];

        Rigidbody productRb = product.GetComponent<Rigidbody>();
        if (productRb != null)
        {
            productRb.isKinematic = true;
            productRb.velocity = Vector3.zero;
            productRb.angularVelocity = Vector3.zero;
        }

        Collider productCollider = product.GetComponent<Collider>();
        if (productCollider != null && boxCollider != null)
        {
            Physics.IgnoreCollision(boxCollider, productCollider, true);
        }

        product.transform.SetParent(slot);
        product.transform.position = slot.position;
        product.transform.rotation = Quaternion.identity;
        product.transform.localScale = Vector3.one;

        placedProducts.Add(product);
        return true;
    }

    public Product TryRemoveProduct(Vector3 rayOrigin, Vector3 rayDirection, float maxDistance)
    {
        if (isBeingCarried) return null;

        LayerMask pickupLayer = LayerMask.GetMask("Pickup");
        Ray ray = new Ray(rayOrigin, rayDirection);
        if (Physics.Raycast(ray, out RaycastHit hit, maxDistance, pickupLayer))
        {
            Product product = hit.collider.GetComponent<Product>();
            if (product != null && placedProducts.Contains(product))
            {
                placedProducts.Remove(product);
                product.transform.SetParent(null);

                Collider productCollider = product.GetComponent<Collider>();
                if (productCollider != null && boxCollider != null)
                {
                    Physics.IgnoreCollision(boxCollider, productCollider, false);
                }

                Rigidbody productRb = product.GetComponent<Rigidbody>();
                if (productRb != null)
                {
                    productRb.isKinematic = false;
                    productRb.velocity = Vector3.zero;
                    productRb.angularVelocity = Vector3.zero;
                }

                product.transform.localScale = product.GetOriginalScale();
                return product;
            }
        }

        return null;
    }

    public void OnPickedUp()
    {
        isBeingCarried = true;
        rb.isKinematic = true;
    }

    public void OnDropped()
    {
        isBeingCarried = false;
        rb.isKinematic = false;
    }

    public bool IsBeingCarried()
    {
        return isBeingCarried;
    }

    public bool IsFull()
    {
        return placedProducts.Count >= productSlots.Length;
    }
}
