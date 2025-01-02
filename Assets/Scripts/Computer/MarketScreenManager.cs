using UnityEngine;

public class MarketScreenManager : MonoBehaviour
{
    public GameObject mainMenu;  // Ana ekran paneli
    public GameObject buyPanel; // Sat�n alma ekran�
    public GameObject sellPanel; // Sat�� ekran�

    // Ana men�y� g�ster
    public void ShowMainMenu()
    {
        mainMenu.SetActive(true);
        buyPanel.SetActive(false);
        sellPanel.SetActive(false);
    }

    // Sat�n alma ekran�n� g�ster
    public void ShowBuyPanel()
    {
        mainMenu.SetActive(false);
        buyPanel.SetActive(true);
        sellPanel.SetActive(false);
    }

    // Sat�� ekran�n� g�ster
    public void ShowSellPanel()
    {
        mainMenu.SetActive(false);
        buyPanel.SetActive(false);
        sellPanel.SetActive(true);
    }
}
