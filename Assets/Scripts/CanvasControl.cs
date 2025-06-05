using UnityEngine;

public class CanvasControl : MonoBehaviour
{
    [SerializeField] private GameObject canvas;
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    [SerializeField] private float interactionDistance = 2f;
    [SerializeField] private Transform player;
    [SerializeField] private MarketScreenManager marketScreenManager;

    private bool isPlayerNearby = false;
    private bool isCanvasOpen = false;

    void Start()
    {
        try
        {
            if (canvas == null)
            {
                Debug.LogError("CanvasControl: Canvas referansý atanmamýþ!");
            }
            else
            {
                canvas.SetActive(false);
                isCanvasOpen = false;
                Debug.Log("Canvas baþlangýçta kapalý.");
            }

            if (player == null) Debug.LogError("CanvasControl: Player Transform atanmamýþ!");
            if (marketScreenManager == null) Debug.LogError("CanvasControl: MarketScreenManager atanmamýþ!");

            LockCursor();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Start sýrasýnda hata: {e.Message}");
        }
    }

    void Update()
    {
        try
        {
            if (isPlayerNearby && Input.GetKeyDown(interactKey))
            {
                Debug.Log($"E tuþuna basýldý, canvas açýlmaya çalýþýlýyor. isPlayerNearby: {isPlayerNearby}");
                ToggleCanvas();
            }

            if (Input.GetKeyDown(KeyCode.Escape) && isCanvasOpen)
            {
                Debug.Log("ESC tuþuna basýldý, canvas kapatýlýyor.");
                CloseCanvas();
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Update sýrasýnda hata: {e.Message}");
        }
    }

    public void ToggleCanvas()
    {
        try
        {
            Debug.Log($"ToggleCanvas çaðrýldý. Mevcut durum: isCanvasOpen={isCanvasOpen}, canvas={(canvas == null ? "null" : canvas.name)}, marketScreenManager={(marketScreenManager == null ? "null" : marketScreenManager.name)}");
            isCanvasOpen = !isCanvasOpen;

            if (canvas == null)
            {
                Debug.LogError("Canvas null, açýlamadý!");
                isCanvasOpen = false;
                return;
            }

            canvas.SetActive(isCanvasOpen);
            Debug.Log($"Canvas durumu oldu: {isCanvasOpen}");

            if (isCanvasOpen)
            {
                if (marketScreenManager != null)
                {
                    marketScreenManager.ShowMainMenu();
                    Debug.Log("MarketScreenManager.ShowMainMenu çaðrýldý.");
                }
                else
                {
                    Debug.LogWarning("MarketScreenManager null, ShowMainMenu çaðrýlmadý!");
                }
                UnlockCursor();
            }
            else
            {
                if (marketScreenManager != null)
                {
                    marketScreenManager.CloseCanvas();
                    Debug.Log("MarketScreenManager.CloseCanvas çaðrýldý.");
                }
                else
                {
                    Debug.LogWarning("MarketScreenManager null, CloseCanvas çaðrýlmadý!");
                }
                LockCursor();
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"ToggleCanvas sýrasýnda hata: {e.Message}");
            isCanvasOpen = false;
        }
    }

    public void CloseCanvas()
    {
        try
        {
            isCanvasOpen = false;
            if (canvas != null)
            {
                canvas.SetActive(false);
                Debug.Log("Canvas kapatýldý.");
            }
            else
            {
                Debug.LogError("Canvas null, kapatýlmadý!");
            }

            if (marketScreenManager != null)
            {
                marketScreenManager.CloseCanvas();
                Debug.Log("MarketScreenManager.CloseCanvas çaðrýldý.");
            }
            else
            {
                Debug.LogWarning("MarketScreenManager null, CloseCanvas çaðrýlmadý!");
            }
            LockCursor();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"CloseCanvas sýrasýnda hata: {e.Message}");
        }
    }

    void OnTriggerEnter(Collider other)
    {
        try
        {
            if (other.CompareTag("Player"))
            {
                isPlayerNearby = true;
                Debug.Log($"Player entered trigger. isPlayerNearby: {isPlayerNearby}, Collider: {other.gameObject.name}");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"OnTriggerEnter sýrasýnda hata: {e.Message}");
        }
    }

    void OnTriggerExit(Collider other)
    {
        try
        {
            if (other.CompareTag("Player"))
            {
                isPlayerNearby = false;
                Debug.Log($"Player exited trigger. isPlayerNearby: {isPlayerNearby}, Collider: {other.gameObject.name}");
                if (isCanvasOpen)
                {
                    CloseCanvas();
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"OnTriggerExit sýrasýnda hata: {e.Message}");
        }
    }

    public void LockCursor()
    {
        try
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            Debug.Log("Fare kilitlendi.");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"LockCursor sýrasýnda hata: {e.Message}");
        }
    }

    public void UnlockCursor()
    {
        try
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            Debug.Log("Fare serbest býrakýldý.");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"UnlockCursor sýrasýnda hata: {e.Message}");
        }
    }
}