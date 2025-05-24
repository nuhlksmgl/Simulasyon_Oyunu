using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Text;

public class OrderListPanelUI : MonoBehaviour
{
    public CustomerOrderManager customerOrderManager;
    public GameObject siparisSatiriPrefab;
    public Transform scrollviewContentParent;
    public PackingStation packingStation; // ✳️ Yeni

    public void Show()
    {
        gameObject.SetActive(true);

        foreach (Transform child in scrollviewContentParent)
        {
            Destroy(child.gameObject);
        }

        List<OrderData> orders = customerOrderManager.GetPendingOrders();

        foreach (OrderData order in orders)
        {
            GameObject satir = Instantiate(siparisSatiriPrefab, scrollviewContentParent);

            TextMeshProUGUI siparisNoText = satir.transform.Find("SiparisNo")?.GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI urunAdlariText = satir.transform.Find("UrunAdlari")?.GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI siparisDurumuText = satir.transform.Find("SiparisDurumu")?.GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI toplamTutarText = satir.transform.Find("ToplamTutar")?.GetComponent<TextMeshProUGUI>();
            Button hazirlaButton = satir.transform.Find("HazirlaButton")?.GetComponent<Button>();

            if (siparisNoText != null)
                siparisNoText.text = $"Sipariş No: {order.orderID}";

            if (urunAdlariText != null)
            {
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < order.itemsInOrder.Count; i++)
                {
                    var item = order.itemsInOrder[i];
                    builder.Append($"{item.quantity} x {item.productDefinition.productName}");
                    if (i < order.itemsInOrder.Count - 1)
                        builder.Append(", ");
                }
                urunAdlariText.text = builder.ToString();
            }

            if (siparisDurumuText != null)
                siparisDurumuText.text = $"Durum: {order.status}";

            if (toplamTutarText != null)
                toplamTutarText.text = $"Tutar: {order.totalOrderValue} ₺";

            if (hazirlaButton != null)
            {
                hazirlaButton.onClick.RemoveAllListeners();
                hazirlaButton.onClick.AddListener(() =>
                {
                    ActiveOrderManager.Instance.SetActiveOrder(order);
                    if (packingStation != null)
                    {
                        packingStation.SpawnCargoBoxForOrder(order);
                    }
                    gameObject.SetActive(false);
                });
            }
        }
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }
}
