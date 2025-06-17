using UnityEngine;

// Bu script'i kargolarý teslim edeceðiniz alandaki bir Trigger'a ekleyin.
public class ShippingZone : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        CargoBox cargoBox = other.GetComponent<CargoBox>();

        if (cargoBox != null)
        {
            // Kutunun bir sipariþi olduðundan emin ol
            if (cargoBox.assignedOrder == null)
            {
                Debug.LogWarning("Bu kutuya bir sipariþ fiþi atanmamýþ.");
                return;
            }

            // GÜNCELLEME: Artýk kutunun kendisinden ceza/ödül bilgisini alýyoruz.
            float reputationChange = cargoBox.CalculatePackingPenalty();

            if (reputationChange == 0) // Eðer hiç ceza yoksa, bu mükemmel bir pakettir.
            {
                // Baþarý puanýný CustomerOrderManager'dan al
                StoreReputation.Instance.AddReputation(CustomerOrderManager.Instance.reputationForSuccess);
            }
            else // Ceza varsa, hesaplanan ceza puanýný uygula
            {
                StoreReputation.Instance.AddReputation(reputationChange);
            }

            // Sipariþi "Tamamlandý" olarak iþaretle (stoktan düþme burada gerçekleþir)
            CustomerOrderManager.Instance.CompleteOrder(cargoBox.assignedOrder.orderID);

            // Kargo kutusunu ve içindekileri yok et
            Destroy(other.gameObject);
        }
    }
}