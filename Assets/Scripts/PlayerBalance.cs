using UnityEngine;
using TMPro;

public class PlayerBalance : MonoBehaviour
{
    public static PlayerBalance Instance { get; private set; }

    [SerializeField] private float startingBalance = 1000f;
    [SerializeField] private TextMeshProUGUI balanceText;

    private float currentBalance;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(this.gameObject); }
        else { Instance = this; }
    }

    void Start()
    {
        currentBalance = startingBalance;
        UpdateBalanceUI();
    }

    private void UpdateBalanceUI()
    {
        if (balanceText != null) { balanceText.text = $"Bakiyen: {currentBalance:F2}$"; }
    }

    public void AddBalance(float amount)
    {
        if (amount > 0)
        {
            currentBalance += amount;
            UpdateBalanceUI();
        }
    }

    // Lisans hatasýný çözen hali
    public bool DeductBalance(float amount)
    {
        if (amount >= 0 && currentBalance >= amount)
        {
            currentBalance -= amount;
            UpdateBalanceUI();
            return true;
        }
        else
        {
            Debug.LogWarning("Yetersiz bakiye veya geçersiz tutar!");
            return false;
        }
    }

    public float GetBalance()
    {
        return currentBalance;
    }
}