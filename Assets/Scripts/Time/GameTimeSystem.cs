using UnityEngine;
using TMPro;

public class GameTimeSystem : MonoBehaviour
{
    [Header("Time Settings")]
    [SerializeField] private float gameMinutePerRealSecond = 1f;
    [SerializeField] private int startingHour = 8;
    [SerializeField] private int startingDay = 1;

    private TMP_Text timeText;
    private TMP_Text dayText;

    private float currentGameMinute;
    private int currentGameHour;
    private int currentGameDay;
    private float minuteTimer;

    public static GameTimeSystem Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Text elementlerini bul
        timeText = GameObject.Find("TimeText")?.GetComponent<TMP_Text>();
        dayText = GameObject.Find("DayText")?.GetComponent<TMP_Text>();

        // Referanslarý kontrol et
        if (timeText == null || dayText == null)
        {
            Debug.LogError("TimeText veya DayText bulunamadý!");
            return;
        }

        // Baþlangýç deðerlerini ayarla
        currentGameHour = startingHour;
        currentGameDay = startingDay;
        currentGameMinute = 0;
        minuteTimer = 0;

        try
        {
            // Ýlk deðerleri göster
            timeText.text = string.Format("{0:00}:{1:00}", currentGameHour, currentGameMinute);
            dayText.text = $"Gün {currentGameDay}";
            Debug.Log("Baþlangýç deðerleri atandý: " + timeText.text);
        }
        catch (System.Exception e)
        {
            Debug.LogError("Format hatasý: " + e.Message);
        }
    }

    private void Update()
    {
        UpdateGameTime();
    }

    private void UpdateGameTime()
    {
        minuteTimer += Time.deltaTime;

        if (minuteTimer >= (1f / gameMinutePerRealSecond))
        {
            minuteTimer = 0;
            IncrementMinute();
        }
    }

    private void IncrementMinute()
    {
        currentGameMinute++;

        if (currentGameMinute >= 60)
        {
            currentGameMinute = 0;
            currentGameHour++;

            if (currentGameHour >= 24)
            {
                currentGameHour = 0;
                currentGameDay++;
            }
        }

        try
        {
            // Zamaný güncelle
            timeText.text = string.Format("{0:00}:{1:00}", currentGameHour, (int)currentGameMinute);
            dayText.text = $"Gün {currentGameDay}";
        }
        catch (System.Exception e)
        {
            Debug.LogError("Format hatasý: " + e.Message);
        }
    }
}