using UnityEngine;

public class MarketScreenManager : MonoBehaviour
{
    public GameObject mainMenu;
    public GameObject buyPanel;
    public GameObject sellPanel;
    public GameObject orderListPanel; // Sipariş listesi paneli için referans

    public InGameMarket inGameMarket;
    public SellPanel sellPanelScript;
    public OrderListPanelUI orderListPanelUI; // Sipariş panelini kontrol edecek script referansı

    void Start()
    {
        // Başlangıçta tüm panelleri kapatmak iyi bir pratik olabilir.
        // CanvasControl script'i zaten canvas'ı kapatıyorsa, burası sadece panelleri kapatır.
        CloseAllPanels();
    }

    // Tüm UI panellerini kapatmak için yardımcı bir metod
    void CloseAllPanels()
    {
        mainMenu?.SetActive(false);
        buyPanel?.SetActive(false);
        sellPanel?.SetActive(false);
        orderListPanel?.SetActive(false);
        // Varsa diğer paneller de buraya eklenebilir (örn: SiparisDetayPaneli)
    }

    public void ShowMainMenu()
    {
        CloseAllPanels();
        if (mainMenu != null) mainMenu.SetActive(true);
        else Debug.LogError("MarketScreenManager: MainMenu referansı atanmamış!");

        // Ana menüye dönüldüğünde SellPanel'in güncel olması iyi olabilir.
        if (sellPanelScript != null)
        {
            // sellPanelScript.UpdateSellPanel(); // Gerekliyse veya SellPanel OnEnable'da güncelleniyorsa
        }
        // Debug.Log("Ana menü gösterildi.");
    }

    public void ShowBuyPanel()
    {
        CloseAllPanels();
        if (buyPanel != null) buyPanel.SetActive(true);
        else Debug.LogError("MarketScreenManager: BuyPanel referansı atanmamış!");

        if (inGameMarket != null)
        {
            inGameMarket.SetupBuyButtons(); // Toptancı butonlarını ayarla
            inGameMarket.UpdatePriceUI_BuyPanel(); // Toptancı fiyatlarını güncelle
        }
        else Debug.LogError("MarketScreenManager: InGameMarket referansı atanmamış!");
        // Debug.Log("Satın alma paneli gösterildi.");
    }

    public void ShowSellPanel()
    {
        CloseAllPanels();
        if (sellPanel != null) sellPanel.SetActive(true);
        else Debug.LogError("MarketScreenManager: SellPanel referansı atanmamış!");

        if (sellPanelScript != null)
        {
            sellPanelScript.UpdateSellPanel(); // Satış panelini açarken UI'ı güncelle
        }
        else Debug.LogError("MarketScreenManager: SellPanelScript referansı atanmamış!");
        // Debug.Log("Satış paneli gösterildi.");
    }

    public void ShowOrderListPanel()
    {
        CloseAllPanels();
        if (orderListPanel != null) orderListPanel.SetActive(true);
        else Debug.LogError("MarketScreenManager: OrderListPanel referansı atanmamış!");

        if (orderListPanelUI != null)
        {
            // OrderListPanelUI'ın Show metodu zaten RefreshOrderList'i veya
            // OnEnable üzerinden listeyi güncellemeyi tetikliyor olmalı.
            orderListPanelUI.Show();
        }
        else
        {
            Debug.LogError("MarketScreenManager: OrderListPanelUI referansı atanmamış!");
        }
        // Debug.Log("Sipariş listesi paneli gösterildi.");
    }

    // Bu metod, CanvasControl tarafından tüm bilgisayar arayüzü kapatıldığında çağrılır.
    public void CloseCanvas()
    {
        CloseAllPanels(); // Tüm alt panelleri kapat

        // Toptancıdan sepete eklenen ürünlerin işlenmesi (kutuların spawn edilmesi)
        // Canvas kapatıldığında gerçekleşir.
        if (inGameMarket != null)
        {
            // --- HATA BURADAYDI: ProcessOrders -> ProcessPurchaseBasket olarak değiştirildi ---
            inGameMarket.ProcessPurchaseBasket();
            // --- DÜZELTME SONU ---
        }
        else
        {
            Debug.LogError("MarketScreenManager - CloseCanvas: InGameMarket referansı atanmamış, toptancı sepeti işlenemedi!");
        }
        // Debug.Log("MarketScreenManager: CloseCanvas çağrıldı, tüm paneller kapatıldı ve toptancı sepeti işlendi.");
    }
}
