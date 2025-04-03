using UnityEngine;

public class MarketScreenManager : MonoBehaviour
{
    public GameObject mainMenu;
    public GameObject buyPanel;
    public GameObject sellPanel;
    public InGameMarket inGameMarket;
    public SellPanel sellPanelScript;

    void Start()
    {
        CloseCanvas();
    }

    public void ShowMainMenu()
    {
        mainMenu.SetActive(true);
        buyPanel.SetActive(false);
        sellPanel.SetActive(false);

        if (sellPanelScript != null)
        {
            sellPanelScript.UpdateSellPanel();
        }
        Debug.Log("Ana menü gösterildi.");
    }

    public void ShowBuyPanel()
    {
        mainMenu.SetActive(false);
        buyPanel.SetActive(true);
        sellPanel.SetActive(false);

        if (inGameMarket != null)
        {
            inGameMarket.SetupBuyButtons();
        }
        Debug.Log("Satýn alma paneli gösterildi.");
    }

    public void ShowSellPanel()
    {
        mainMenu.SetActive(false);
        buyPanel.SetActive(false);
        sellPanel.SetActive(true);

        if (sellPanelScript != null)
        {
            sellPanelScript.UpdateSellPanel();
        }
        Debug.Log("Satýþ paneli gösterildi.");
    }

    public void CloseCanvas()
    {
        mainMenu.SetActive(false);
        buyPanel.SetActive(false);
        sellPanel.SetActive(false);

        if (inGameMarket != null)
        {
            inGameMarket.ProcessOrders();
        }
        Debug.Log("Canvas kapatýldý, sipariþler iþlendi.");
    }
}