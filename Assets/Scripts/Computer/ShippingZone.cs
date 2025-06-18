// FileName: ShippingZone.cs
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class ShippingZone : MonoBehaviour
{
    private void Awake()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<CargoBox>(out CargoBox cargoBox))
        {
            if (cargoBox.assignedOrder != null)
            {
                Debug.Log($"Kargo alanýna {cargoBox.assignedOrder.orderID} ID'li sipariþ kutusu býrakýldý. Ýþleniyor...");
                if (CustomerOrderManager.Instance != null)
                {
                    CustomerOrderManager.Instance.ProcessShippedOrder(cargoBox);
                }
                else
                {
                    Debug.LogError("CustomerOrderManager.Instance bulunamadý! Sipariþ iþlenemedi.");
                }
                Destroy(cargoBox.gameObject);
            }
            else
            {
                Debug.LogWarning("Kargo alanýna sipariþi olmayan bir kutu býrakýldý. Bir iþlem yapýlmadý.");
            }
        }
    }
}