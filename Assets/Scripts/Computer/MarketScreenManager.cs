using UnityEngine;

public class MarketScreenManager : MonoBehaviour
{
    [SerializeField] private GameObject mainMenu;
    [SerializeField] private GameObject buyPanel;
    [SerializeField] private GameObject sellPanel;
    [SerializeField] private GameObject orderListPanel;

    [SerializeField] private InGameMarket inGameMarket;
    [SerializeField] private SellPanel sellPanelScript;
    [SerializeField] private OrderListPanelUI orderListPanelUI;

    void Awake()
    {
        if (mainMenu == null || buyPanel == null || sellPanel == null || orderListPanel == null)
        {
            Debug.LogError("MarketScreenManager: Panel referanslarından biri eksik!");
        }
        if (inGameMarket == null || sellPanelScript == null || orderListPanelUI == null)
        {
            Debug.LogError("MarketScreenManager: Script referanslarından biri eksik!");
        }
    }

    void Start()
    {
        CloseAllPanels();
    }

    void CloseAllPanels()
    {
        mainMenu?.SetActive(false);
        buyPanel?.SetActive(false);
        sellPanel?.SetActive(false);
        orderListPanel?.SetActive(false);
    }

    public void ShowMainMenu()
    {
        CloseAllPanels();
        if (mainMenu != null) mainMenu.SetActive(true);
        else Debug.LogError("MarketScreenManager: MainMenu referansı atanmamış!");
    }

    public void ShowBuyPanel()
    {
        CloseAllPanels();
        if (buyPanel != null) buyPanel.SetActive(true);
        else Debug.LogError("MarketScreenManager: BuyPanel referansı atanmamış!");

        if (inGameMarket != null)
        {
            inGameMarket.SetupBuyButtons();
            inGameMarket.UpdatePriceUI_BuyPanel();
        }
        else Debug.LogError("MarketScreenManager: InGameMarket referansı atanmamış!");
    }

    public void ShowSellPanel()
    {
        CloseAllPanels();
        if (sellPanel != null) sellPanel.SetActive(true);
        else Debug.LogError("MarketScreenManager: SellPanel referansı atanmamış!");

        if (sellPanelScript != null)
        {
            sellPanelScript.UpdateSellPanel();
        }
        else Debug.LogError("MarketScreenManager: SellPanelScript referansı atanmamış!");
    }

    public void ShowOrderListPanel()
    {
        CloseAllPanels();
        if (orderListPanel != null) orderListPanel.SetActive(true);
        else Debug.LogError("MarketScreenManager: OrderListPanel referansı atanmamış!");

        if (orderListPanelUI != null)
        {
            orderListPanelUI.Show();
        }
        else Debug.LogError("MarketScreenManager: OrderListPanelUI referansı atanmamış!");
    }

    public void CloseCanvas()
    {
        CloseAllPanels();
        if (inGameMarket != null)
        {
            inGameMarket.ProcessPurchaseBasket();
        }
        else
        {
            Debug.LogError("MarketScreenManager: InGameMarket referansı atanmamış, toptancı sepeti işlenemedi!");
        }
        Debug.Log("MarketScreenManager: CloseCanvas çağrıldı, tüm paneller kapatıldı ve toptancı sepeti işlendi.");
    }
}