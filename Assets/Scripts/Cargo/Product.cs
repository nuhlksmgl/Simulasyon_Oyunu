using UnityEngine;

public class Product : MonoBehaviour
{
    public InGameMarket.MarketProduct productDefinition; // 📌 Ürün tanımı → sipariş eşleştirme için

    private ObjectPickup objectPickup;
    public bool isPlaced = false;
    public bool isHeld = false;

    private Vector3 originalScale;

    private void Awake()
    {
        originalScale = transform.localScale;
    }

    private void Start()
    {
        objectPickup = FindObjectOfType<ObjectPickup>();
        gameObject.tag = "Pickup";
    }

    // Kargo kutusuna yerleştirme kontrolü (başka bir sistemden çağrılır)
    public void TryPlaceInNearbyBox()
    {
        CargoBox[] boxes = FindObjectsOfType<CargoBox>();
        foreach (CargoBox box in boxes)
        {
            if (box.IsInRange(transform.position) && box.TryPlaceProduct(this))
            {
                isPlaced = true;
                isHeld = false;
                objectPickup.ClearHeldObject();
                return;
            }
        }
    }

    public void OnPickedUp()
    {
        isHeld = true;
        isPlaced = false;
    }

    public void ResetPosition()
    {
        isPlaced = false;
        isHeld = false;
        transform.localScale = originalScale;
    }

    public Vector3 GetOriginalScale()
    {
        return originalScale;
    }
}
