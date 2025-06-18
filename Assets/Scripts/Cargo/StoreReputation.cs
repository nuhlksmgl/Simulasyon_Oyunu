// StoreReputation.cs
using UnityEngine;
using TMPro; // Eðer puaný UI'da göstereceksen
// using UnityEngine.UI; // Eðer bar kullanacaksan

public class StoreReputation : MonoBehaviour
{
    public static StoreReputation Instance { get; private set; }

    [Header("Ýtibar Ayarlarý")]
    [Range(0f, 100f)]
    public float currentReputation = 70f; // Baþlangýç itibarý (0-100 arasý)
    public float maxReputation = 100f;
    public float minReputation = 0f; // Ýflas için kritik eþik (veya 0)
    public float reputationThresholdForBankruptcy = 10f; // Bu deðerin altýna düþünce iflas uyarýsý/riski

    [Header("UI (Opsiyonel)")]
    public TextMeshProUGUI reputationText; // Puaný gösterecek UI elemaný
    // public Image reputationBar; // Puaný bar olarak gösterecek UI elemaný (þimdilik yorumda)

    // Event: Ýtibar puaný deðiþtiðinde tetiklenir.
    // Parametre olarak yeni itibar puanýný gönderir.
    public static event System.Action<float> OnReputationChanged;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // DontDestroyOnLoad(gameObject); // Oyun boyunca tek bir instancesta kalmasý gerekiyorsa
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Baþlangýçta UI'ý güncelle ve event'i tetikle (diðer sistemler baþlangýç deðerini alsýn)
        UpdateReputationUI();
        OnReputationChanged?.Invoke(currentReputation);
    }

    /// <summary>
    /// Maðaza itibarýný belirli bir miktar artýrýr veya azaltýr.
    /// </summary>
    /// <param name="amount">Eklenecek veya çýkarýlacak itibar miktarý (negatif olabilir).</param>
    public void AddReputation(float amount)
    {
        currentReputation = Mathf.Clamp(currentReputation + amount, minReputation, maxReputation);
        Debug.Log($"ÝTÝBAR DEÐÝÞTÝ: {amount:+0.0;-0.0}. Yeni Ýtibar: {currentReputation:F1}");

        UpdateReputationUI();
        OnReputationChanged?.Invoke(currentReputation); // Event'i tetikle

        // Ýflas Kontrolü
        if (currentReputation <= reputationThresholdForBankruptcy)
        {
            HandleLowReputation();
        }
    }

    void UpdateReputationUI()
    {
        if (reputationText != null)
        {
            reputationText.text = $"{currentReputation:F1}"; // / {maxReputation}";
        }
        // if (reputationBar != null)
        // {
        //     reputationBar.fillAmount = currentReputation / maxReputation;
        // }
    }

    void HandleLowReputation()
    {
        // Bu fonksiyon itibar çok düþtüðünde çaðrýlýr.
        // Ýflas mekaniði burada tetiklenebilir veya oyuncuya ciddi uyarýlar verilebilir.
        if (currentReputation <= minReputation)
        {
            Debug.LogWarning("MAÐAZA ÝTÝBARI SIFIRLANDI! ÝFLAS RÝSKÝ ÇOK YÜKSEK!");
            // Burada oyun sonu veya iflas senaryosu baþlatýlabilir.
            // Örneðin: Time.timeScale = 0; FindObjectOfType<UIManager>()?.ShowGameOverScreen("Ýtibar Kaybý Nedeniyle Ýflas!");
        }
        else
        {
            Debug.LogWarning($"DÝKKAT: Maðaza itibarý kritik seviyede: {currentReputation:F1}!");
            // Oyuncuya uyarý mesajlarý gösterilebilir (Chirper sistemiyle vb.)
        }
    }

    public float GetCurrentReputation()
    {
        return currentReputation;
    }
}