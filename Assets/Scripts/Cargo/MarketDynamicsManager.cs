using UnityEngine;

public class MarketDynamicsManager : MonoBehaviour
{
    [Header("Baðlantýlar")]
    public InGameMarket inGameMarket;

    [Header("Dalgalanma Ayarlarý")]
    [Tooltip("Her ürün için fiyatýn günlük olarak deðiþme olasýlýðý (0 ile 1 arasýnda).")]
    [Range(0f, 1f)]
    public float priceChangeChanceDaily = 0.8f; // Olasýlýðý artýrdým, daha sýk deðiþim için

    [Tooltip("Fiyatýn baseMarketPrice etrafýnda minimum yüzde kaç dalgalanabileceði (örn: 0.05 = +/- %5).")]
    [Range(0.01f, 0.2f)]
    public float minFluctuationPercentage = 0.05f; // Minimum %5

    [Tooltip("Fiyatýn baseMarketPrice etrafýnda maksimum yüzde kaç dalgalanabileceði (örn: 0.10 = +/- %10).")]
    [Range(0.05f, 0.25f)]
    public float maxFluctuationPercentage = 0.15f; // Maksimum dalgalanmayý biraz artýrdým

    private const int MAX_CONSECUTIVE_TREND_DAYS = 3;

    void Start()
    {
        if (inGameMarket == null)
        {
            Debug.LogError("MarketDynamicsManager: InGameMarket referansý ATANMAMIÞ! Script devre dýþý býrakýlýyor.");
            enabled = false;
            return;
        }
        if (inGameMarket.products == null || inGameMarket.products.Length == 0)
        {
            Debug.LogWarning("MarketDynamicsManager: InGameMarket'te güncellenecek ürün bulunmuyor. Ürünleri InGameMarket'e eklediðinizden emin olun.");
            enabled = false;
            return;
        }

        // Önce InGameMarket'in kendi initialize metodunun çaðrýldýðýndan emin olalým (eðer varsa)
        // veya direkt burada initialize edelim.
        InitializeAllProductMarketPrices();

        TimeManager.OnNewDayStarted += HandleNewDay;
        Debug.Log("MarketDynamicsManager BAÞLATILDI ve TimeManager.OnNewDayStarted event'ine abone oldu.");

        // Oyun baþlar baþlamaz bir fiyat güncellemesi yapmak için (0. gün etkisi)
        // Bu, oyuncunun oyuna her baþladýðýnda farklý piyasa koþullarýyla karþýlaþmasýný saðlayabilir.
        // HandleNewDay(); // Eðer ilk gün hemen bir dalgalanma isteniyorsa bu satýrý açýn.
    }

    void InitializeAllProductMarketPrices()
    {
        Debug.Log("MarketDynamicsManager: Tüm ürünlerin piyasa fiyatlarý initialize ediliyor/kontrol ediliyor...");
        bool allPricesValid = true;
        foreach (InGameMarket.MarketProduct product in inGameMarket.products)
        {
            if (product == null)
            {
                Debug.LogWarning("MarketDynamicsManager: InGameMarket.products içinde null bir ürün var.");
                allPricesValid = false;
                continue;
            }

            // 1. baseMarketPrice'ý ayarla
            if (product.baseMarketPrice <= 0)
            {
                if (product.price > 0)
                {
                    product.baseMarketPrice = Mathf.Round(product.price * 1.5f); // Alýþ fiyatýnýn %50 fazlasý
                    Debug.LogWarning($"{product.productName} için baseMarketPrice ({product.baseMarketPrice}) Inspector'dan ayarlanmamýþ veya 0. Alýþ fiyatýndan ({product.price}$) hesaplandý: {product.baseMarketPrice}$");
                }
                else
                {
                    product.baseMarketPrice = 50f; // Güvenlik için varsayýlan
                    Debug.LogError($"{product.productName} için hem baseMarketPrice hem de alýþ fiyatý geçersiz! baseMarketPrice varsayýlan olarak {product.baseMarketPrice}$ ayarlandý.");
                    allPricesValid = false;
                }
            }

            // 2. currentAverageMarketPrice'ý baseMarketPrice'a eþitle (eðer baþlangýçta 0 ise veya base'den çok farklýysa)
            if (product.currentAverageMarketPrice <= 0 || Mathf.Approximately(product.currentAverageMarketPrice, 0))
            {
                product.currentAverageMarketPrice = product.baseMarketPrice;
                // Debug.Log($"{product.productName} için currentAverageMarketPrice, baseMarketPrice'a eþitlendi.");
            }
            Debug.Log($"INIT/KONTROL: {product.productName} -> Alýþ: {product.price}$, Taban Piyasa: {product.baseMarketPrice:F0}$, Güncel Piyasa: {product.currentAverageMarketPrice:F0}$");
        }
        if (!allPricesValid)
        {
            Debug.LogError("MarketDynamicsManager: Bazý ürünlerin fiyat bilgileri geçersiz veya eksik! Lütfen InGameMarket ayarlarýný kontrol edin.");
        }
    }

    void OnDestroy()
    {
        TimeManager.OnNewDayStarted -= HandleNewDay;
    }

    void HandleNewDay()
    {
        Debug.Log("<color=cyan>MarketDynamicsManager: HandleNewDay ÇAÐRILDI (Yeni Gün Event'i alýndý)!</color>");
        UpdateMarketPricesDaily();
    }

    void UpdateMarketPricesDaily()
    {
        if (inGameMarket == null || inGameMarket.products == null)
        {
            Debug.LogError("MarketDynamicsManager - UpdateMarketPricesDaily: InGameMarket veya ürünleri null!");
            return;
        }
        Debug.Log("<color=blue>MarketDynamicsManager: UpdateMarketPricesDaily BAÞLADI.</color>");

        int changedPricesCount = 0;
        foreach (InGameMarket.MarketProduct product in inGameMarket.products)
        {
            if (product == null) { Debug.LogWarning("MarketDynamicsManager: Güncellenecek ürün listesinde null bir ürün var."); continue; }
            if (product.baseMarketPrice <= 0)
            {
                Debug.LogWarning($"MarketDynamicsManager: {product.productName} için baseMarketPrice ({product.baseMarketPrice}) geçerli deðil, fiyat dalgalanmasý atlanýyor.");
                continue;
            }

            if (Random.value < priceChangeChanceDaily)
            {
                float oldMarketPrice = product.currentAverageMarketPrice;
                float potentialDirection = (Random.value < 0.5f) ? -1f : 1f;

                if (product.priceTrendStreak >= MAX_CONSECUTIVE_TREND_DAYS && potentialDirection > 0)
                {
                    potentialDirection = -1f;
                    Debug.Log($"TREND KIRMA: {product.productName} max yükseliþ serisinde ({product.priceTrendStreak} gün), düþmeye zorlanýyor.");
                }
                else if (product.priceTrendStreak <= -MAX_CONSECUTIVE_TREND_DAYS && potentialDirection < 0)
                {
                    potentialDirection = 1f;
                    Debug.Log($"TREND KIRMA: {product.productName} max düþüþ serisinde ({product.priceTrendStreak} gün), yükselmeye zorlanýyor.");
                }

                float actualFluctuationPercentage = Random.Range(minFluctuationPercentage, maxFluctuationPercentage);
                float referencePriceForFluctuation = product.baseMarketPrice;

                float fluctuationAmount = referencePriceForFluctuation * actualFluctuationPercentage * potentialDirection;
                float newMarketPrice = referencePriceForFluctuation + fluctuationAmount;

                newMarketPrice = Mathf.Max(product.price * 1.05f, newMarketPrice);
                newMarketPrice = Mathf.Round(newMarketPrice);

                if (!Mathf.Approximately(oldMarketPrice, newMarketPrice))
                {
                    if (newMarketPrice > oldMarketPrice)
                    {
                        product.priceTrendStreak = (product.priceTrendStreak > 0) ? product.priceTrendStreak + 1 : 1;
                    }
                    else
                    {
                        product.priceTrendStreak = (product.priceTrendStreak < 0) ? product.priceTrendStreak - 1 : -1;
                    }
                    Debug.Log($"FÝYAT DEÐÝÞTÝ: {product.productName} | Eski: {oldMarketPrice:F0}$ | Yeni: {newMarketPrice:F0}$ | Yön: {(potentialDirection > 0 ? "Yükseliþ" : "Düþüþ")} | Dalgalanma: {actualFluctuationPercentage:P0} | TrendSeri: {product.priceTrendStreak}");
                    product.currentAverageMarketPrice = newMarketPrice;
                    changedPricesCount++;
                }
                else
                {
                    product.priceTrendStreak = 0;
                    // Debug.Log($"{product.productName} için fiyat etkin olarak deðiþmedi, trend sýfýrlandý.");
                }
            }
            else
            {
                product.priceTrendStreak = 0;
                // Debug.Log($"{product.productName} için fiyat deðiþtirme þansý ({priceChangeChanceDaily:P0}) tutmadý, trend sýfýrlandý.");
            }
        }

        Debug.Log($"<color=blue>MarketDynamicsManager: UpdateMarketPricesDaily TAMAMLANDI. {changedPricesCount} ürünün fiyatý güncellendi.</color>");

        SellPanel sellPanelInstance = FindObjectOfType<SellPanel>();
        if (sellPanelInstance != null && sellPanelInstance.gameObject.activeInHierarchy)
        {
            sellPanelInstance.UpdateSellPanel();
        }
    }
}
