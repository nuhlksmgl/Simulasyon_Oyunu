using UnityEngine;

public class Product : MonoBehaviour
{
    public InGameMarket.MarketProduct productDefinition;
    public bool isHeld = false;  // Eksik olan değişken eklendi
    private Vector3 originalScale;

    private void Awake()
    {
        originalScale = transform.localScale;
        gameObject.tag = "Pickup";
    }

    public void OnPickedUp()
    {
        isHeld = true;
    }

    public void ResetPosition()
    {
        isHeld = false;
        transform.localScale = originalScale;
    }

    public Vector3 GetOriginalScale()
    {
        return originalScale;
    }
}
