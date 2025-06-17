using UnityEngine;
using UnityEngine.SceneManagement; // Sahne yönetimi için gerekli

public class MainMenuManager : MonoBehaviour
{
    // "Yeni Oyun" butonuna atanacak metot
    public void StartNewGame()
    {
        // "GameScene" adýný kendi oyun sahnenizin adýyla deðiþtirin
        SceneManager.LoadScene("GameScene");
    }

    // "Ayarlar" butonuna atanacak metot
    public void OpenSettings()
    {
        // Þimdilik boþ, daha sonra ayarlar panelini açacak kod buraya gelebilir.
        Debug.Log("Ayarlar menüsü açýlýyor...");
    }

    // "Çýkýþ" butonuna atanacak metot
    public void ExitGame()
    {
        Debug.Log("Oyundan çýkýlýyor...");
        Application.Quit();
    }
}