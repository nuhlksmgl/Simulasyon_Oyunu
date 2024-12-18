using UnityEngine;

public class Product : MonoBehaviour
{
    private ObjectPickup objectPickup;
    private bool isPlaced = false;
    public bool isHeld = false; // �r�n�n tutulup tutulmad���n� takip eden de�i�ken

    private void Start()
    {
        objectPickup = FindObjectOfType<ObjectPickup>();
        gameObject.tag = "Pickup"; // �r�n�n etiketini "Pickup" yapar
    }

    private void Update()
    {
        // �r�n elde tutuluyorsa ve 'F' tu�una bas�ld�ysa kargo kutusuna yerle�tir
        if (!isPlaced && isHeld && objectPickup != null && objectPickup.GetHeldObject() == gameObject && Input.GetKeyDown(KeyCode.F))
        {
            TryPlaceInBox();
        }
    }

    // �r�n� kargo kutusuna yerle�tirmeyi dener
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
                Debug.Log($"{gameObject.name} kargo kutusuna yerle�tirildi.");
                break;
            }
        }

        if (!isPlaced)
        {
            Debug.Log($"{gameObject.name} kutuya yerle�tirilemedi.");
        }
    }

    public void OnPickedUp()
    {
        isHeld = true; // �r�n al�nd�
        isPlaced = false;
        Debug.Log($"{gameObject.name} al�nd�.");
    }

    public void ResetPosition()
    {
        isPlaced = false;
        isHeld = false;
    }
}

// Rigidbody'yi fiziksel etkile�ime
