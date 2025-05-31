using UnityEngine;
using TMPro;

public class TimeManager : MonoBehaviour
{
    // Event: Yeni bir gün baþladýðýnda tetiklenir.
    public static event System.Action OnNewDayStarted;

    [Header("Zaman Ayarlarý")]
    [Tooltip("Oyun zamanýnýn gerçek zamana göre ne kadar hýzlý akacaðýný belirler. 1 = Gerçek zamanlý, 60 = Gerçek 1 saniye oyun içi 1 dakika.")]
    public float timeMultiplier = 60f;

    [Header("Baþlangýç Zamaný")]
    [Range(0, 23)]
    public int startHour = 7;    // Baþlangýç saati
    [Range(0, 59)]
    public int startMinute = 0;  // Baþlangýç dakikasý
    public int startDay = 1;     // Baþlangýç günü

    // Oyun zamaný (dýþarýdan sadece okunabilir)
    public int CurrentHour { get; private set; }
    public int CurrentMinute { get; private set; }
    public int CurrentDay { get; private set; }

    [Header("UI Referanslarý")]
    public TMP_Text timeText;
    public TMP_Text dayText;

    private float secondsPassedInGameTime = 0f; // Oyun baþladýðýndan beri geçen toplam oyun saniyesi

    void Awake()
    {
        // Baþlangýç deðerlerini ata
        CurrentHour = startHour;
        CurrentMinute = startMinute;
        CurrentDay = startDay;

        // Baþlangýç zamanýný toplam saniyeye çevir (oyun zamaný cinsinden)
        secondsPassedInGameTime = (CurrentHour * 3600f) + (CurrentMinute * 60f);
    }

    void Start()
    {
        UpdateUI(); // Baþlangýç UI'ýný ayarla
        Debug.Log($"TimeManager Baþlatýldý. Gün: {CurrentDay}, Saat: {CurrentHour:D2}:{CurrentMinute:D2}");
        // Ýlk gün için event'i tetikle (eðer bazý sistemler baþlangýçta gün bilgisine ihtiyaç duyuyorsa)
        // OnNewDayStarted?.Invoke(); // Veya ilk günün zaten baþladýðý varsayýlýr.
    }

    void Update()
    {
        UpdateGameTime();
        UpdateUI();
    }

    void UpdateGameTime()
    {
        // Gerçek saniyeyi oyun zamaný çarpanýyla artýrarak oyun saniyesini ilerlet
        secondsPassedInGameTime += Time.deltaTime * timeMultiplier;

        // Toplam oyun saniyesinden güncel saat, dakika ve günü hesapla
        int totalGameSeconds = Mathf.FloorToInt(secondsPassedInGameTime);

        int previousDay = CurrentDay; // Gün deðiþimi kontrolü için

        CurrentDay = startDay + (totalGameSeconds / 86400); // 86400 saniye = 24 saat
        int secondsInCurrentDay = totalGameSeconds % 86400;

        CurrentHour = secondsInCurrentDay / 3600;
        int secondsInCurrentHour = secondsInCurrentDay % 3600;
        CurrentMinute = secondsInCurrentHour / 60;
        // int currentSecond = secondsInCurrentHour % 60; // Saniyeyi de istersen tutabilirsin

        if (CurrentDay > previousDay)
        {
            Debug.Log($"YENÝ GÜN BAÞLADI: Gün {CurrentDay}");
            OnNewDayStarted?.Invoke(); // Yeni gün event'ini tetikle
        }
    }

    void UpdateUI()
    {
        if (timeText != null)
        {
            string formattedTime = string.Format("{0:00}:{1:00}", CurrentHour, CurrentMinute);
            timeText.text = "Saat: " + formattedTime;
        }

        if (dayText != null)
        {
            dayText.text = "Gün: " + CurrentDay;
        }
    }

    /// <summary>
    /// Mevcut oyun içi zamaný toplam dakika cinsinden döndürür (günün baþýndan itibaren).
    /// CustomerOrderManager gibi script'ler sipariþ zaman damgalarý için kullanabilir.
    /// </summary>
    public float GetCurrentTimeInMinutesOfDay()
    {
        return CurrentHour * 60f + CurrentMinute;
    }

    /// <summary>
    /// Oyun baþladýðýndan beri geçen toplam oyun içi dakikayý döndürür.
    /// </summary>
    public float GetTotalMinutesPassedInGame()
    {
        return secondsPassedInGameTime / 60f;
    }
}