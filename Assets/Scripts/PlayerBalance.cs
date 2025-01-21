using UnityEngine;
using TMPro;

public class PlayerBalance : MonoBehaviour
{
    public int balance = 100; // Oyuncunun ba�lang�� bakiyesi
    public TextMeshProUGUI balanceText; // Bakiyeyi g�sterecek TextMeshPro UI eleman�

    void Start()
    {
        UpdateBalanceUI(); // Oyuncu bakiyesini g�ncelle
    }

    // Bakiyeyi art�rma fonksiyonu
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
            return true; // ��lem ba�ar�l�
        }
        else
        {
            Debug.Log("Yeterli bakiyeniz yok!"); // Yetersiz bakiye mesaj�
            return false; // ��lem ba�ar�s�z
        }
    }

    // UI �zerindeki bakiyeyi g�ncelle
    void UpdateBalanceUI()
    {
        balanceText.text = $"Bakiyen: {balance} $";
    }

    // Bakiyeyi d�nd�ren fonksiyon
    public int GetBalance()
    {
        return balance;
    }
}
