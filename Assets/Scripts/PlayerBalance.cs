using UnityEngine;
using TMPro;

public class PlayerBalance : MonoBehaviour
{
    public int balance = 100; // Oyuncunun baþlangýç bakiyesi
    public TextMeshProUGUI balanceText; // Bakiyeyi gösterecek TextMeshPro UI elemaný

    void Start()
    {
        UpdateBalanceUI(); // Oyuncu bakiyesini güncelle
    }

    // Bakiyeyi artýrma fonksiyonu
    public void AddBalance(int amount)
    {
        balance += amount;
        UpdateBalanceUI();
    }

    // Bakiyeyi azaltma fonksiyonu
    public bool DeductBalance(int amount)
    {
        if (balance >= amount) // Yeterli bakiye varsa
        {
            balance -= amount;
            UpdateBalanceUI();
            return true; // Ýþlem baþarýlý
        }
        else
        {
            Debug.Log("Yeterli bakiyeniz yok!"); // Yetersiz bakiye mesajý
            return false; // Ýþlem baþarýsýz
        }
    }

    // UI üzerindeki bakiyeyi güncelle
    void UpdateBalanceUI()
    {
        balanceText.text = $"Bakiyen: {balance} $";
    }

    // Bakiyeyi döndüren fonksiyon
    public int GetBalance()
    {
        return balance;
    }
}
