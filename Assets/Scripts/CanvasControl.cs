using UnityEngine;

public class CanvasControl : MonoBehaviour
{
    [Header("UI Ayarları")]
    [SerializeField] private GameObject computerCanvas; // İsim daha anlaşılır olması için değiştirildi
    [SerializeField] private KeyCode interactKey = KeyCode.E;

    [Header("Etkileşim Ayarları")]
    [SerializeField] private Transform player;
    [SerializeField] private float interactionDistance = 3f;

    // YENİ EKLENDİ: Kamera referansları
    [Header("Kamera Ayarları")]
    [SerializeField] private Camera mainCamera; // Oyuncunun ana kamerası
    [SerializeField] private Camera uiCamera;   // Bilgisayar ekranına bakan kamera

    [Header("UI Elemanları")]
    [SerializeField] private GameObject crosshairUI;

    [Header("Yönetici Referansları")]
    [SerializeField] private MarketScreenManager marketScreenManager;

    public static bool IsUiOpen { get; private set; }

    void Start()
    {
        // GÜNCELLEME: Başlangıçta UI kamerasının da kapalı olduğundan emin ol
        if (computerCanvas != null) computerCanvas.SetActive(false);
        if (uiCamera != null) uiCamera.gameObject.SetActive(false);
        if (mainCamera != null) mainCamera.gameObject.SetActive(true);

        if (crosshairUI != null) crosshairUI.SetActive(true);
        IsUiOpen = false;
        LockCursor();
    }

    void Update()
    {
        // UI kapalıyken E'ye basılmasını dinle
        if (!IsUiOpen)
        {
            if (IsPlayerInDistance() && Input.GetKeyDown(interactKey))
            {
                OpenCanvas();
            }
        }
        // UI açıkken ESC'ye basılmasını dinle
        else
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                CloseCanvas();
            }
        }
    }

    // GÜNCELLENMİŞ METOT
    public void OpenCanvas()
    {
        IsUiOpen = true;

        // Kameraları değiştir
        mainCamera?.gameObject.SetActive(false);
        uiCamera?.gameObject.SetActive(true);

        // UI'ı aç ve crosshair'ı kapat
        computerCanvas?.SetActive(true);
        marketScreenManager?.ShowMainMenu();
        if (crosshairUI != null) crosshairUI.SetActive(false);

        UnlockCursor();
    }

    // GÜNCELLENMİŞ METOT
    public void CloseCanvas()
    {
        IsUiOpen = false;

        // Kameraları eski haline getir
        uiCamera?.gameObject.SetActive(false);
        mainCamera?.gameObject.SetActive(true);

        // UI'ı kapat ve crosshair'ı aç
        computerCanvas?.SetActive(false);
        if (crosshairUI != null) crosshairUI.SetActive(true);

        LockCursor();
    }

    private bool IsPlayerInDistance()
    {
        if (player == null) return false;
        return Vector3.Distance(player.position, transform.position) <= interactionDistance;
    }

    void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}