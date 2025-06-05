namespace Simulasyon.Computer
{
    using UnityEngine;
    using TMPro;
    using UnityEngine.UI;
    using System.Text;

    public class SlipDetailsUI : MonoBehaviour
    {
        [SerializeField] private GameObject slipDetailsPanel;
        [SerializeField] private TextMeshProUGUI orderIDText;
        [SerializeField] private TextMeshProUGUI customerIDText;
        [SerializeField] private TextMeshProUGUI orderTypeText;
        [SerializeField] private TextMeshProUGUI itemsText;
        [SerializeField] private TextMeshProUGUI totalValueText;
        [SerializeField] private Button closeButton;

        void Awake()
        {
            if (slipDetailsPanel == null) Debug.LogError("SlipDetailsPanel atanmamış!");
            if (orderIDText == null || customerIDText == null || orderTypeText == null || itemsText == null || totalValueText == null)
            {
                Debug.LogError("SlipDetailsUI: TextMeshPro alanlarından biri eksik!");
            }
            if (closeButton == null) Debug.LogError("CloseButton atanmamış!");

            closeButton.onClick.AddListener(Hide);
            slipDetailsPanel.SetActive(false);
        }

        public void Show(OrderData order)
        {
            if (order == null)
            {
                Debug.LogError("SlipDetailsUI: Order verisi null!");
                return;
            }

            try
            {
                orderIDText.text = $"Sipariş ID: {order.orderID}";
                customerIDText.text = $"Müşteri No: {order.customerName}";
                orderTypeText.text = $"Sipariş Tipi: {order.orderType}";
                totalValueText.text = $"Toplam Tutar: {order.totalOrderValue}₺";

                StringBuilder builder = new StringBuilder();
                foreach (var item in order.itemsInOrder)
                {
                    builder.AppendLine($"- {item.productDefinition?.productName ?? "[Bilinmeyen Ürün]"} x{item.quantity} ({item.unitSellPriceAtOrderTime}₺)");
                }
                itemsText.text = builder.ToString();

                slipDetailsPanel.SetActive(true);
                Debug.Log($"SlipDetailsUI: Sipariş ID {order.orderID} detayları gösterildi.");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"SlipDetailsUI Show sırasında hata: {e.Message}");
            }
        }

        public void Hide()
        {
            slipDetailsPanel.SetActive(false);
            Debug.Log("SlipDetailsUI: Panel kapatıldı.");
        }
    }
}