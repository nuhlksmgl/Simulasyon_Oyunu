using UnityEngine;
using UnityEngine.UI;

public class MarketScreenManager : MonoBehaviour
{
    [Header("Paneller")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject buyPanel;
    [SerializeField] private GameObject sellPanel;
    [SerializeField] private GameObject shoppingCartPanel; // YENİ EKLENDİ
    [SerializeField] private GameObject orderPanel;
    // deliveryPanel kaldırıldı

    // Market ilk açıldığında veya Geri butonlarıyla Ana Menü'ye dönüldüğünde çağrılır.
    public void ShowMainMenu()
    {
        mainMenuPanel?.SetActive(true);
        buyPanel?.SetActive(false);
        sellPanel?.SetActive(false);
        shoppingCartPanel?.SetActive(false);
        orderPanel?.SetActive(false);
    }

    // "Buy" butonuna tıklandığında bu metot çalışacak
    public void OpenBuyPanel()
    {
        mainMenuPanel?.SetActive(false);
        buyPanel?.SetActive(true);
    }

    // "Sell" butonuna tıklandığında bu metot çalışacak
    public void OpenSellPanel()
    {
        mainMenuPanel?.SetActive(false);
        sellPanel?.SetActive(true);
    }

    // Sağ üstteki sepet butonuna tıklandığında çağrılacak YENİ METOT
    public void OpenCartPanel()
    {
        buyPanel?.SetActive(false);
        sellPanel?.SetActive(false);
        mainMenuPanel?.SetActive(false);
        shoppingCartPanel?.SetActive(true);
    }

    // Sepetten geri gelmek için kullanılacak YENİ METOT
    public void BackToBuyPanel()
    {
        shoppingCartPanel?.SetActive(false);
        buyPanel?.SetActive(true);
    }

    // Order Panelini açmak için kullanılacak YENİ METOT
    public void OpenOrderPanel()
    {
        mainMenuPanel?.SetActive(false);
        buyPanel?.SetActive(false);
        sellPanel?.SetActive(false);
        shoppingCartPanel?.SetActive(false);
        orderPanel?.SetActive(true);
    }

    // Market arayüzünü tamamen kapatır
    public void CloseCanvas()
    {
        this.gameObject.SetActive(false);
    }
}
