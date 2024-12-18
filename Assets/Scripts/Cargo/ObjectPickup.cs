using UnityEngine;

public class ObjectPickup : MonoBehaviour
{
    public Transform holdPosition; // Ürünün elde tutulacaðý pozisyon
    private GameObject heldObject = null;

    void Update()
    {
        if (Input.GetMouseButtonDown(0)) // Sol mouse tuþuyla ürünü al ve býrak
        {
            if (heldObject == null)
            {
                TryPickupObject();
            }
            else
            {
                DropObject();
            }
        }

        UpdateHeldObjectPosition();
    }

    // Ürünü elde tutulan pozisyona yumuþakça geçiþ yapar
    private void UpdateHeldObjectPosition()
    {
        if (heldObject != null && holdPosition != null)
        {
            heldObject.transform.position = Vector3.Lerp(
                heldObject.transform.position,
                holdPosition.position,
                Time.deltaTime * 20f);

            heldObject.transform.rotation = holdPosition.rotation;
        }
    }

    // Ürünü almayý dener
    void TryPickupObject()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 5.0f))
        {
            if (hit.collider.gameObject.CompareTag("Pickup"))
            {
                heldObject = hit.collider.gameObject;

                // Ürünü holdPosition altýnda parent olarak ayarla
                heldObject.transform.SetParent(holdPosition);

                // Rigidbody'yi ayarla
                Rigidbody rb = heldObject.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.isKinematic = true; // Ürünün fiziksel olarak yere düþmesini engelle
                }

                // Product'ý bilgilendir
                Product product = heldObject.GetComponent<Product>();
                if (product != null)
                {
                    product.OnPickedUp();
                }

                Debug.Log($"Picked up: {heldObject.name}");
            }
        }
    }

    // Ürünü býrakýr
    void DropObject()
    {
        if (heldObject != null)
        {
            // Rigidbody'yi fiziksel etkileþime açýk hale getir
            Rigidbody rb = heldObject.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = false; // Ürün yere düþebilir hale gelsin
                rb.velocity = Vector3.zero; // Hareket hýzýný sýfýrla
                rb.angularVelocity = Vector3.zero; // Dönme hýzýný sýfýrla
            }

            // Parent baðlantýsýný kaldýr ve dünya konumunda serbest býrak
            heldObject.transform.SetParent(null, true); // Dünya konumunda serbest býrak (true kullanýlarak)
            heldObject.transform.localPosition = heldObject.transform.position; // Mevcut pozisyonunu koru

            // Product scriptindeki durumlarý güncelle
            Product product = heldObject.GetComponent<Product>();
            if (product != null)
            {
                product.isHeld = false; // Ürün artýk tutulmuyor
                product.ResetPosition(); // Ürünün durumunu sýfýrla
            }

            Debug.Log($"Dropped: {heldObject.name}");
            heldObject = null; // heldObject referansýný temizle
        }
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
