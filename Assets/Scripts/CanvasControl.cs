using UnityEngine;

public class CanvasControl : MonoBehaviour
{
    public GameObject canvas; // Canvas referansý
    public KeyCode interactKey = KeyCode.E; // Etkileþim tuþu
    public float interactionDistance = 2f; // Mesafe kontrolü için kullanýlabilir (isteðe baðlý)
    public Transform player; // Oyuncunun Transform'u
    public bool isCursorLocked = true; // Ýmleç kilitli mi?

    private bool isPlayerNearby = false; // Oyuncunun yakýnlýk durumunu kontrol etmek için

    void Start()
    {
        LockCursor(); // Oyun baþladýðýnda imleci gizle ve kilitle
    }

    void Update()
    {
        // Eðer oyuncu yakýnsa ve etkileþim tuþuna basarsa
        if (isPlayerNearby && Input.GetKeyDown(interactKey))
        {
            ToggleCanvas();
        }

        // ESC tuþu ile imleci serbest býrak
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            UnlockCursor();
        }

        // Eðer Canvas kapalýysa imleci tekrar kilitlemek için
        if (!canvas.activeSelf && isCursorLocked == false)
        {
            LockCursor();
        }
    }

    void ToggleCanvas()
    {
        bool isActive = canvas.activeSelf; // Canvas'ýn açýk olup olmadýðýný kontrol et
        canvas.SetActive(!isActive); // Durumu tersine çevir

        if (canvas.activeSelf)
        {
            UnlockCursor(); // Canvas açýldýðýnda imleci serbest býrak
        }
        else
        {
            LockCursor(); // Canvas kapandýðýnda imleci tekrar kilitle
        }
    }

    // Oyuncu Collider'ýn içine girerse
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) // Oyuncunun etiketinin "Player" olduðundan emin olun
        {
            isPlayerNearby = true; // Oyuncu yakýn
        }
    }

    // Oyuncu Collider'dan çýkarsa
    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player")) // Oyuncu ayrýldýðýnda
        {
            isPlayerNearby = false; // Oyuncu uzak
        }
    }

    // Ýmleci kilitle ve gizle
    void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        isCursorLocked = true;
    }

    // Ýmleci serbest býrak ve görünür yap
    void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        isCursorLocked = false;
    }
}
