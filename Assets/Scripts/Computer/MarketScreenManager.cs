using UnityEngine;

public class MarketScreenManager : MonoBehaviour
{
    public GameObject mainMenu;
    public GameObject buyPanel;
    public GameObject sellPanel;
    public InGameMarket inGameMarket;

    void Start()
    {
        // Oyun baþladýðýnda fareyi gizle ve kilitle (isteðe baðlý)
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    public void ShowMainMenu()
    {
        mainMenu.SetActive(true);
        buyPanel.SetActive(false);
        sellPanel.SetActive(false);
        Cursor.visible = true; // Canvas açýkken fare görünür
        Cursor.lockState = CursorLockMode.None;
    }

    public void ShowBuyPanel()
    {
        mainMenu.SetActive(false);
        buyPanel.SetActive(true);
        sellPanel.SetActive(false);
        Cursor.visible = true; // Canvas açýkken fare görünür
        Cursor.lockState = CursorLockMode.None;
    }

    public void ShowSellPanel()
    {
        mainMenu.SetActive(false);
        buyPanel.SetActive(false);
        sellPanel.SetActive(true);
        Cursor.visible = true; // Canvas açýkken fare görünür
        Cursor.lockState = CursorLockMode.None;
    }

    public void CloseCanvas()
    {
        mainMenu.SetActive(false);
        buyPanel.SetActive(false);
        sellPanel.SetActive(false);
        inGameMarket.ProcessOrders(); // Sipariþleri iþle
        Cursor.visible = false; // Canvas kapandýðýnda fareyi gizle
        Cursor.lockState = CursorLockMode.Locked; // Fareyi kilitle
    }
}