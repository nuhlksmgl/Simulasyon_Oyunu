using UnityEngine;

public class MarketScreenManager : MonoBehaviour
{
    public GameObject mainMenu;
    public GameObject buyPanel;
    public GameObject sellPanel;
    public GameObject orderListPanel;

    public InGameMarket inGameMarket;
    public SellPanel sellPanelScript;
    public OrderListPanelUI orderListPanelUI; // Sipariş panelini kontrol edecek script referansı

    void Start()
    {
        CloseCanvas();
    }

    public void ShowMainMenu()
    {
        mainMenu.SetActive(true);
        buyPanel.SetActive(false);
        sellPanel.SetActive(false);
        orderListPanel.SetActive(false);

        if (sellPanelScript != null)
        {
            sellPanelScript.UpdateSellPanel();
        }
    }

    public void ShowBuyPanel()
    {
        mainMenu.SetActive(false);
        buyPanel.SetActive(true);
        sellPanel.SetActive(false);
        orderListPanel.SetActive(false);

        if (inGameMarket != null)
        {
            inGameMarket.SetupBuyButtons();
        }
    }

    public void ShowSellPanel()
    {
        mainMenu.SetActive(false);
        buyPanel.SetActive(false);
        sellPanel.SetActive(true);
        orderListPanel.SetActive(false);

        if (sellPanelScript != null)
        {
            sellPanelScript.UpdateSellPanel();
        }
    }

    public void ShowOrderListPanel()
    {
        mainMenu.SetActive(false);
        buyPanel.SetActive(false);
        sellPanel.SetActive(false);
        orderListPanel.SetActive(true);

        if (orderListPanelUI != null)
        {
            orderListPanelUI.Show(); // 💥 Siparişler burada gösteriliyor!
        }
    }

    public void CloseCanvas()
    {
        mainMenu.SetActive(false);
        buyPanel.SetActive(false);
        sellPanel.SetActive(false);
        orderListPanel.SetActive(false);

        if (inGameMarket != null)
        {
            inGameMarket.ProcessOrders();
        }
    }
}
