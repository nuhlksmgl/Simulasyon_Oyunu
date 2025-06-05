using UnityEngine;

public class Product : MonoBehaviour
{
    public InGameMarket.MarketProduct productDefinition;
    public bool isHeld = false;
    private Vector3 originalWorldScale;

    private void Awake()
    {
        // Instantiate edildiğinde localScale = lossyScale, çünkü ebeveyn yok
        originalWorldScale = transform.localScale;
        transform.localScale = originalWorldScale; // Prefab ölçeğini sıfırla
        Debug.Log($"Product {name} orijinal dünya ölçeği kaydedildi: {originalWorldScale}");

        gameObject.tag = "Pickup";

        if (productDefinition == null)
        {
            Debug.LogWarning($"Product {name} için productDefinition atanmamış!");
        }
    }

    public void OnPickedUp()
    {
        isHeld = true;
        Debug.Log($"Product {name} alındı.");
    }

    public void ResetPosition()
    {
        isHeld = false;
        transform.localScale = originalWorldScale;
        Debug.Log($"Product {name} pozisyonu sıfırlandı, ölçek: {originalWorldScale}");
    }

    public Vector3 GetOriginalWorldScale()
    {
        Debug.Log($"Product {name} orijinal dünya ölçeği döndürüyor: {originalWorldScale}");
        return originalWorldScale;
    }
}