using UnityEngine;

public class ObjectPickup : MonoBehaviour
{
    public Transform holdPosition; // �r�n�n elde tutulaca�� pozisyon
    private GameObject heldObject = null;

    void Update()
    {
        if (Input.GetMouseButtonDown(0)) // Sol mouse tu�uyla �r�n� al ve b�rak
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

    // �r�n� elde tutulan pozisyona yumu�ak�a ge�i� yapar
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

    // �r�n� almay� dener
    void TryPickupObject()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 5.0f))
        {
            if (hit.collider.gameObject.CompareTag("Pickup"))
            {
                heldObject = hit.collider.gameObject;

                // �r�n� holdPosition alt�nda parent olarak ayarla
                heldObject.transform.SetParent(holdPosition);

                // Rigidbody'yi ayarla
                Rigidbody rb = heldObject.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.isKinematic = true; // �r�n�n fiziksel olarak yere d��mesini engelle
                }

                // Product'� bilgilendir
                Product product = heldObject.GetComponent<Product>();
                if (product != null)
                {
                    product.OnPickedUp();
                }

                Debug.Log($"Picked up: {heldObject.name}");
            }
        }
    }

    // �r�n� b�rak�r
    void DropObject()
    {
        if (heldObject != null)
        {
            // Rigidbody'yi fiziksel etkile�ime a��k hale getir
            Rigidbody rb = heldObject.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = false; // �r�n yere d��ebilir hale gelsin
                rb.velocity = Vector3.zero; // Hareket h�z�n� s�f�rla
                rb.angularVelocity = Vector3.zero; // D�nme h�z�n� s�f�rla
            }

            // Parent ba�lant�s�n� kald�r ve d�nya konumunda serbest b�rak
            heldObject.transform.SetParent(null, true); // D�nya konumunda serbest b�rak (true kullan�larak)
            heldObject.transform.localPosition = heldObject.transform.position; // Mevcut pozisyonunu koru

            // Product scriptindeki durumlar� g�ncelle
            Product product = heldObject.GetComponent<Product>();
            if (product != null)
            {
                product.isHeld = false; // �r�n art�k tutulmuyor
                product.ResetPosition(); // �r�n�n durumunu s�f�rla
            }

            Debug.Log($"Dropped: {heldObject.name}");
            heldObject = null; // heldObject referans�n� temizle
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
