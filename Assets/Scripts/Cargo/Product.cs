using UnityEngine;
public class Product : MonoBehaviour
{
    private ObjectPickup objectPickup;
    public bool isPlaced = false; // Erişilebilir hale getirdik
    public bool isHeld = false;   // Ürün tutulup tutulmadığını takip eden değişken

    private void Start()
    {
        objectPickup = FindObjectOfType<ObjectPickup>();
        gameObject.tag = "Pickup"; // Ürünün etiketini "Pickup" yapar
    }

    private void Update()
    {
        // Ürün elde tutuluyorsa ve 'F' tuşuna basıldığında kargo kutusuna yerleştir
        if (!isPlaced && isHeld && objectPickup != null && objectPickup.GetHeldObject() == gameObject && Input.GetKeyDown(KeyCode.F))
        {
            TryPlaceInBox();
        }
    }

    // Ürünü kargo kutusuna yerleştirmeyi dener
    private void TryPlaceInBox()
    {
        CargoBox[] boxes = FindObjectsOfType<CargoBox>();
        foreach (CargoBox box in boxes)
        {
            if (box.IsInRange(transform.position) && box.TryPlaceProduct(this))
            {
                isPlaced = true;
                isHeld = false;
                objectPickup.ClearHeldObject();
                Debug.Log($"{gameObject.name} kargo kutusuna yerleştirildi.");
                break;
            }
        }

        if (!isPlaced)
        {
            Debug.Log($"{gameObject.name} kutuya yerleştirilemedi.");
        }
    }

    public void OnPickedUp()
    {
        isHeld = true; // Ürün alındı
        isPlaced = false;
        Debug.Log($"{gameObject.name} alındı.");
    }

    public void ResetPosition()
    {
        isPlaced = false;
        isHeld = false;
    }
}