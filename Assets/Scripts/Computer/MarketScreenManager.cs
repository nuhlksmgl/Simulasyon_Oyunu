using UnityEngine;
using UnityEngine.UI;

public class MarketScreenManager : MonoBehaviour
{
    [Header("Paneller")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject buyPanel;
    [SerializeField] private GameObject sellPanel;
    [SerializeField] private GameObject orderPanel;
    [SerializeField] private GameObject deliveryPanel;

    // Market ilk açıldığında bu metot çağrılır.
    public void ShowMainMenu()
    {
        // Ana menüyü göster, diğerlerini gizle
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
        if (buyPanel != null) buyPanel.SetActive(false);
        if (sellPanel != null) sellPanel.SetActive(false);
        if (orderPanel != null) orderPanel.SetActive(false);
        if (deliveryPanel != null) deliveryPanel.SetActive(false);
    }

    // "Buy" butonuna tıklandığında bu metot çalışacak
    public void OpenBuyPanel()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (buyPanel != null) buyPanel.SetActive(true);
    }

    // "Sell" butonuna tıklandığında bu metot çalışacak
    public void OpenSellPanel()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (sellPanel != null) sellPanel.SetActive(true);
    }

    // YENİ EKLENEN METOT
    // Bu metot, market arayüzünü tamamen kapatmak için çağrılır.
    public void CloseCanvas()
    {
        // Bu script'in bağlı olduğu ana GameObject'i kapatır.
        this.gameObject.SetActive(false);
    }
}