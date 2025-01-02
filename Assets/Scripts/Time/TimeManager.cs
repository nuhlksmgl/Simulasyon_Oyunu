using UnityEngine;
using TMPro;

public class TimeManager : MonoBehaviour
{
    // Zaman ayarlar�
    public float timeMultiplier = 60f; // Oyun zaman� h�z� (�rne�in, ger�ek 1 saniye = oyun i�i 1 dakika)

    // Oyun zaman�
    private int currentHour = 7;       // Ba�lang�� saati
    private int currentMinute = 0;    // Ba�lang�� dakikas�
    private int currentDay = 1;       // Ba�lang�� g�n�

    // UI i�in referanslar
    public TMP_Text timeText;         // Zaman� g�stermek i�in TextMeshPro referans�
    public TMP_Text dayText;          // G�n� g�stermek i�in TextMeshPro referans�

    private float elapsedTime = 0f;   // Ge�en s�re (zaman�n ilerleyi�ini takip eder)
    private bool hasDayChanged = false; // G�n de�i�imini kontrol etmek i�in bayrak

    void Start()
    {
        // Ba�lang�� zaman� ayarla
        elapsedTime = (currentHour * 60) * 60f; // Saatleri saniyeye �evirip ba�lang�� zaman� olarak ekle
        currentMinute = 0; // Dakikalar ba�lang��ta s�f�r
        currentDay = 1;    // Ba�lang�� g�n� 1

        // Ba�lang�� zaman bilgilerini konsola yazd�r
        Debug.Log("Game Started. Day: " + currentDay + ", Hour: " + currentHour + ", Minute: " + currentMinute);
    }

    void Update()
    {
        UpdateGameTime();  // Oyun i�i zaman� g�ncelle
        UpdateUI();        // UI'yi g�ncelle
    }

    void UpdateGameTime()
    {
        // Ge�en s�reyi hesapla
        elapsedTime += Time.deltaTime * timeMultiplier;

        // Dakika ve saat hesaplama
        int totalMinutes = Mathf.FloorToInt(elapsedTime / 60); // Toplam saniyeyi dakikaya �evir
        currentMinute = totalMinutes % 60;
        currentHour = (totalMinutes / 60) % 24;

        // G�n de�i�imi kontrol�
        if (currentHour == 0 && currentMinute == 0 && !hasDayChanged)
        {
            currentDay++;
            hasDayChanged = true; // G�n de�i�imini tetikledikten sonra bayra�� kald�r
            Debug.Log("New Day Started: Day " + currentDay);
        }

        // G�n de�i�im bayra��n� s�f�rla
        if (currentHour != 0 || currentMinute != 0)
        {
            hasDayChanged = false;
        }
    }

    void UpdateUI()
    {
        // Saat ve dakikay� "HH:MM" format�nda ayarla
        string formattedTime = string.Format("{0:00}:{1:00}", currentHour, currentMinute);
        timeText.text = "Time: " + formattedTime;

        // G�n say�s�n� UI'ye yaz
        dayText.text = "Day: " + currentDay;
    }
}
