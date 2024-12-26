using UnityEngine;
using TMPro;

public class TimeManager : MonoBehaviour
{
    // Zaman ayarlarý
    public float timeMultiplier = 60f; // Oyun zamaný hýzý (örneðin, gerçek 1 saniye = oyun içi 1 dakika)

    // Oyun zamaný
    private int currentHour = 7;       // Baþlangýç saati
    private int currentMinute = 0;    // Baþlangýç dakikasý
    private int currentDay = 1;       // Baþlangýç günü

    // UI için referanslar
    public TMP_Text timeText;         // Zamaný göstermek için TextMeshPro referansý
    public TMP_Text dayText;          // Günü göstermek için TextMeshPro referansý

    private float elapsedTime = 0f;   // Geçen süre (zamanýn ilerleyiþini takip eder)
    private bool hasDayChanged = false; // Gün deðiþimini kontrol etmek için bayrak

    void Start()
    {
        // Baþlangýç zamaný ayarla
        elapsedTime = (currentHour * 60) * 60f; // Saatleri saniyeye çevirip baþlangýç zamaný olarak ekle
        currentMinute = 0; // Dakikalar baþlangýçta sýfýr
        currentDay = 1;    // Baþlangýç günü 1

        // Baþlangýç zaman bilgilerini konsola yazdýr
        Debug.Log("Game Started. Day: " + currentDay + ", Hour: " + currentHour + ", Minute: " + currentMinute);
    }

    void Update()
    {
        UpdateGameTime();  // Oyun içi zamaný güncelle
        UpdateUI();        // UI'yi güncelle
    }

    void UpdateGameTime()
    {
        // Geçen süreyi hesapla
        elapsedTime += Time.deltaTime * timeMultiplier;

        // Dakika ve saat hesaplama
        int totalMinutes = Mathf.FloorToInt(elapsedTime / 60); // Toplam saniyeyi dakikaya çevir
        currentMinute = totalMinutes % 60;
        currentHour = (totalMinutes / 60) % 24;

        // Gün deðiþimi kontrolü
        if (currentHour == 0 && currentMinute == 0 && !hasDayChanged)
        {
            currentDay++;
            hasDayChanged = true; // Gün deðiþimini tetikledikten sonra bayraðý kaldýr
            Debug.Log("New Day Started: Day " + currentDay);
        }

        // Gün deðiþim bayraðýný sýfýrla
        if (currentHour != 0 || currentMinute != 0)
        {
            hasDayChanged = false;
        }
    }

    void UpdateUI()
    {
        // Saat ve dakikayý "HH:MM" formatýnda ayarla
        string formattedTime = string.Format("{0:00}:{1:00}", currentHour, currentMinute);
        timeText.text = "Time: " + formattedTime;

        // Gün sayýsýný UI'ye yaz
        dayText.text = "Day: " + currentDay;
    }
}
