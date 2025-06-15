using UnityEngine;

public class CanvasControl : MonoBehaviour
{
    [Header("Ana Ayarlar")]
    [SerializeField] private GameObject canvas;
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    [SerializeField] private Transform player;
    [SerializeField] private MarketScreenManager marketScreenManager;
    [SerializeField] private float interactionDistance = 3f;

    // YENÝ EKLENEN SATIR
    [Header("UI Elemanlarý")]
    [SerializeField] private GameObject crosshairUI; // Crosshair'ýn GameObject'ini buraya atayacaðýz

    public static bool IsUiOpen { get; private set; }

    // ... Start() metodu ayný kalacak ...
    void Start()
    {
        if (canvas != null) canvas.SetActive(false);
        if (crosshairUI != null) crosshairUI.SetActive(true); // Oyun baþýnda crosshair açýk olsun
        IsUiOpen = false;
        LockCursor();
    }


    // ... Update() metodu ayný kalacak ...
    void Update()
    {
        if (!IsUiOpen)
        {
            if (IsPlayerInDistance() && Input.GetKeyDown(interactKey))
            {
                OpenCanvas();
            }
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                CloseCanvas();
            }
        }
    }


    public void OpenCanvas()
    {
        IsUiOpen = true;
        canvas.SetActive(true);
        if (marketScreenManager != null) marketScreenManager.ShowMainMenu();

        // YENÝ EKLENEN SATIR: Menü açýlýnca crosshair'ý kapat
        if (crosshairUI != null) crosshairUI.SetActive(false);

        UnlockCursor();
    }

    public void CloseCanvas()
    {
        IsUiOpen = false;
        if (canvas != null) canvas.SetActive(false);

        // YENÝ EKLENEN SATIR: Menü kapanýnca crosshair'ý geri aç
        if (crosshairUI != null) crosshairUI.SetActive(true);

        LockCursor();
    }

    // ... Script'in geri kalaný ayný kalacak ...
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