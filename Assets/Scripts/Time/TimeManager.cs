using UnityEngine;
using TMPro;

public class TimeManager : MonoBehaviour
{
    // YENÝ EKLENDÝ: Singleton yapýsý için statik referans
    public static TimeManager Instance { get; private set; }

    [Header("Zaman Ayarlarý")]
    [Tooltip("Oyun zamanýnýn gerçek zamana göre ne kadar hýzlý akacaðýný belirler. 1 = Gerçek zamanlý, 60 = Gerçek 1 saniye oyun içi 1 dakika.")]
    public float timeMultiplier = 60f;

    [Header("Baþlangýç Zamaný")]
    [Range(0, 23)]
    public int startHour = 7;
    [Range(0, 59)]
    public int startMinute = 0;
    public int startDay = 1;

    public int CurrentHour { get; private set; }
    public int CurrentMinute { get; private set; }
    public int CurrentDay { get; private set; }

    [Header("UI Referanslarý")]
    public TMP_Text timeText;
    public TMP_Text dayText;

    private float secondsPassedInGameTime = 0f;

    // YENÝ EKLENDÝ: Singleton'ý ayarlar
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
        }

        CurrentHour = startHour;
        CurrentMinute = startMinute;
        CurrentDay = startDay;
        secondsPassedInGameTime = (CurrentHour * 3600f) + (CurrentMinute * 60f);
    }

    void Start()
    {
        UpdateUI();
    }

    void Update()
    {
        UpdateGameTime();
        UpdateUI();
    }

    void UpdateGameTime()
    {
        secondsPassedInGameTime += Time.deltaTime * timeMultiplier;
        int totalGameSeconds = Mathf.FloorToInt(secondsPassedInGameTime);
        int previousDay = CurrentDay;
        CurrentDay = startDay + (totalGameSeconds / 86400);
        int secondsInCurrentDay = totalGameSeconds % 86400;
        CurrentHour = secondsInCurrentDay / 3600;
        int secondsInCurrentHour = secondsInCurrentDay % 3600;
        CurrentMinute = secondsInCurrentHour / 60;

        if (CurrentDay > previousDay)
        {
            // Yeni gün event'i burada tetiklenebilir
        }
    }

    void UpdateUI()
    {
        if (timeText != null)
        {
            timeText.text = "" + string.Format("{0:00}:{1:00}", CurrentHour, CurrentMinute);
        }
        if (dayText != null)
        {
            dayText.text = "" + CurrentDay;
        }
    }

    public float GetCurrentTimeInMinutesOfDay()
    {
        return CurrentHour * 60f + CurrentMinute;
    }

    public float GetTotalMinutesPassedInGame()
    {
        return secondsPassedInGameTime / 60f;
    }
}