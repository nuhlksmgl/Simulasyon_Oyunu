// FileName: PauseMenuManager.cs (Ana Menüye Dönüþ Eklendi)
using UnityEngine;
using UnityEngine.SceneManagement; // << YENÝ EKLENDÝ: Sahne yönetimi için gerekli.

public class PauseMenuManager : MonoBehaviour
{
    [Header("Referanslar")]
    [Tooltip("Durdurma menüsü olarak kullanýlacak UI Paneli.")]
    [SerializeField] private GameObject pauseMenuPanel;

    [Tooltip("Oyun durdurulduðunda devre dýþý býrakýlacak script'ler (oyuncu hareketi, kamera kontrolü vb.).")]
    [SerializeField] private MonoBehaviour[] scriptsToDisableOnPause;

    public static bool isPaused = false;

    private void Start()
    {
        // ... (Bu metodun içeriði ayný, deðiþiklik yok)
        pauseMenuPanel.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        // ... (Bu metodun içeriði ayný, deðiþiklik yok)
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }

    public void PauseGame()
    {
        // ... (Bu metodun içeriði ayný, deðiþiklik yok)
        isPaused = true;
        Time.timeScale = 0f;
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        foreach (var script in scriptsToDisableOnPause)
        {
            if (script != null) script.enabled = false;
        }
    }

    public void ResumeGame()
    {
        // ... (Bu metodun içeriði ayný, deðiþiklik yok)
        isPaused = false;
        Time.timeScale = 1f;
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        foreach (var script in scriptsToDisableOnPause)
        {
            if (script != null) script.enabled = true;
        }
    }

    // --- ESKÝ QuitGame METODU YERÝNE BU GELDÝ ---
    /// <summary>
    /// Oyunu ana menüye döndürür.
    /// </summary>
    public void LoadMainMenu()
    {
        // ÇOK ÖNEMLÝ: Yeni sahneye geçmeden önce zamaný normale döndür,
        // yoksa yeni sahne de "donmuþ" þekilde yüklenebilir.
        Time.timeScale = 1f;
        isPaused = false; // Pause durumunu sýfýrla

        // "MainMenuScene" isimli sahneyi yükle. Týrnak içindeki ismin doðru olduðundan emin ol.
        SceneManager.LoadScene("MainMenuScene");
    }
}