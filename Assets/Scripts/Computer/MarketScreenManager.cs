using UnityEngine;

public class MarketScreenManager : MonoBehaviour
{
    public GameObject mainMenu;  // Ana ekran paneli
    public GameObject buyPanel; // Satýn alma ekraný
    public GameObject sellPanel; // Satýþ ekraný

    // Ana menüyü göster
    public void ShowMainMenu()
    {
        mainMenu.SetActive(true);
        buyPanel.SetActive(false);
        sellPanel.SetActive(false);
    }

    // Satýn alma ekranýný göster
    public void ShowBuyPanel()
    {
        mainMenu.SetActive(false);
        buyPanel.SetActive(true);
        sellPanel.SetActive(false);
    }

    // Satýþ ekranýný göster
    public void ShowSellPanel()
    {
        mainMenu.SetActive(false);
        buyPanel.SetActive(false);
        sellPanel.SetActive(true);
    }
}
