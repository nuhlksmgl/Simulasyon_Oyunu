using UnityEngine;

public class CanvasControl : MonoBehaviour
{
    public GameObject canvas; // Canvas referans�
    public KeyCode interactKey = KeyCode.E; // Etkile�im tu�u
    public float interactionDistance = 2f; // Mesafe kontrol� i�in kullan�labilir (iste�e ba�l�)
    public Transform player; // Oyuncunun Transform'u

    private bool isPlayerNearby = false; // Oyuncunun yak�nl�k durumunu kontrol etmek i�in

    void Update()
    {
        // E�er oyuncu yak�nsa ve etkile�im tu�una basarsa
        if (isPlayerNearby && Input.GetKeyDown(interactKey))
        {
            ToggleCanvas();
        }
    }

    void ToggleCanvas()
    {
        bool isActive = canvas.activeSelf; // Canvas'�n a��k olup olmad���n� kontrol et
        canvas.SetActive(!isActive); // Durumu tersine �evir
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
}
