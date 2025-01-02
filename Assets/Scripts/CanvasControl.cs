using UnityEngine;

public class CanvasControl : MonoBehaviour
{
    public GameObject canvas; // Canvas referans�
    public KeyCode interactKey = KeyCode.E; // Etkile�im tu�u
    public float interactionDistance = 2f; // Mesafe kontrol� i�in kullan�labilir (iste�e ba�l�)
    public Transform player; // Oyuncunun Transform'u
    public bool isCursorLocked = true; // �mle� kilitli mi?

    private bool isPlayerNearby = false; // Oyuncunun yak�nl�k durumunu kontrol etmek i�in

    void Start()
    {
        LockCursor(); // Oyun ba�lad���nda imleci gizle ve kilitle
    }

    void Update()
    {
        // E�er oyuncu yak�nsa ve etkile�im tu�una basarsa
        if (isPlayerNearby && Input.GetKeyDown(interactKey))
        {
            ToggleCanvas();
        }

        // ESC tu�u ile imleci serbest b�rak
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            UnlockCursor();
        }

        // E�er Canvas kapal�ysa imleci tekrar kilitlemek i�in
        if (!canvas.activeSelf && isCursorLocked == false)
        {
            LockCursor();
        }
    }

    void ToggleCanvas()
    {
        bool isActive = canvas.activeSelf; // Canvas'�n a��k olup olmad���n� kontrol et
        canvas.SetActive(!isActive); // Durumu tersine �evir

        if (canvas.activeSelf)
        {
            UnlockCursor(); // Canvas a��ld���nda imleci serbest b�rak
        }
        else
        {
            LockCursor(); // Canvas kapand���nda imleci tekrar kilitle
        }
    }

    // Oyuncu Collider'�n i�ine girerse
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) // Oyuncunun etiketinin "Player" oldu�undan emin olun
        {
            isPlayerNearby = true; // Oyuncu yak�n
        }
    }

    // Oyuncu Collider'dan ��karsa
    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player")) // Oyuncu ayr�ld���nda
        {
            isPlayerNearby = false; // Oyuncu uzak
        }
    }

    // �mleci kilitle ve gizle
    void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        isCursorLocked = true;
    }

    // �mleci serbest b�rak ve g�r�n�r yap
    void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        isCursorLocked = false;
    }
}
