using UnityEngine;

public class CanvasControl : MonoBehaviour
{
    public GameObject canvas; // Canvas referansý
    public KeyCode interactKey = KeyCode.E; // Etkileþim tuþu
    public float interactionDistance = 2f; // Mesafe kontrolü için kullanýlabilir (isteðe baðlý)
    public Transform player; // Oyuncunun Transform'u

    private bool isPlayerNearby = false; // Oyuncunun yakýnlýk durumunu kontrol etmek için

    void Update()
    {
        // Eðer oyuncu yakýnsa ve etkileþim tuþuna basarsa
        if (isPlayerNearby && Input.GetKeyDown(interactKey))
        {
            ToggleCanvas();
        }
    }

    void ToggleCanvas()
    {
        bool isActive = canvas.activeSelf; // Canvas'ýn açýk olup olmadýðýný kontrol et
        canvas.SetActive(!isActive); // Durumu tersine çevir
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
}
