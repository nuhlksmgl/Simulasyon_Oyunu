using UnityEngine;
using TMPro;

public class PlayerBalance : MonoBehaviour
{
    public int balance = 100;
    public TextMeshProUGUI balanceText; 

    void Start()
    {
        UpdateBalanceUI(); 
    }

    
    public void AddBalance(int amount)
    {
        balance += amount;
        UpdateBalanceUI();
    }

    
    public bool DeductBalance(int amount)
    {
        if (balance >= amount) 
        {
            balance -= amount;
            UpdateBalanceUI();
            return true; 
        }
        else
        {
            Debug.Log("Yeterli bakiyeniz yok!"); 
            return false;
        }
    }

    
    void UpdateBalanceUI()
    {
        balanceText.text = $"Bakiyen: {balance} $";
    }

    
    public int GetBalance()
    {
        return balance;
    }
}
