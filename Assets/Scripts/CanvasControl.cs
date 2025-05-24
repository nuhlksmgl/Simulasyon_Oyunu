using UnityEngine;

public class CanvasControl : MonoBehaviour
{
    public GameObject canvas; // Canvas referansý
    public KeyCode interactKey = KeyCode.E; // Etkileþim tuþu
    public float interactionDistance = 2f; // Mesafe kontrolü için kullanýlabilir (isteðe baðlý)
    public Transform player; // Oyuncunun Transform'u
    public MarketScreenManager marketScreenManager; // MarketScreenManager referansý

    private bool isPlayerNearby = false; // Oyuncunun yakýnlýk durumunu kontrol etmek için
    private bool isCanvasOpen = false; // Canvas’ýn açýk olup olmadýðýný takip etmek için

    void Start()
    {
        // Canvas’ý baþlangýçta kapat
        if (canvas != null)
        {
            canvas.SetActive(false);
            isCanvasOpen = false;
        }

        // Fareyi kilitle ve gizle
        LockCursor();
    }

    void Update()
    {
        // Eðer oyuncu yakýnsa ve etkileþim tuþuna basarsa
        if (isPlayerNearby && Input.GetKeyDown(interactKey))
        {
            ToggleCanvas();
        }

        // ESC tuþu ile canvas’ý kapat ve fareyi kilitle
        if (Input.GetKeyDown(KeyCode.Escape) && isCanvasOpen)
        {
            CloseCanvas();
        }
    }

    public void ToggleCanvas()
    {
        isCanvasOpen = !isCanvasOpen;
        canvas.SetActive(isCanvasOpen);

        if (isCanvasOpen)
        {
            // Canvas açýldýðýnda MarketScreenManager’ýn ShowMainMenu metodunu çaðýr
            if (marketScreenManager != null)
            {
                marketScreenManager.ShowMainMenu();
            }
            UnlockCursor();
        }
        else
        {
            // Canvas kapandýðýnda MarketScreenManager’ýn CloseCanvas metodunu çaðýr
            if (marketScreenManager != null)
            {
                marketScreenManager.CloseCanvas();
            }
            LockCursor();
        }
    }

    public void CloseCanvas()
    {
        isCanvasOpen = false;
        canvas.SetActive(false);

        if (marketScreenManager != null)
        {
            marketScreenManager.CloseCanvas();
        }
        LockCursor();
    }

    // Oyuncu Collider’ýn içine girerse
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) // Oyuncunun etiketinin "Player" olduðundan emin olun
        {
            isPlayerNearby = true;
            
        }
    }

    // Oyuncu Collider’dan çýkarsa
    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player")) // Oyuncu ayrýldýðýnda
        {
            isPlayerNearby = false;
            // Oyuncu uzaklaþtýðýnda canvas açýksa kapat
            if (isCanvasOpen)
            {
                CloseCanvas();
            }
            
        }
    }

    // Ýmleci kilitle ve gizle
    public void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
    }

    // Ýmleci serbest býrak ve görünür yap
    public void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
    }
}