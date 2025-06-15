using UnityEngine;

public class Shelf : MonoBehaviour
{
    // Inspector'dan bu rafa ait slotlarý atayacaðýmýz dizi
    public Transform[] itemSlots;

    // Bu raf sahnede aktif olduðunda (oluþturulduðunda) çalýþýr
    void Start()
    {
        // Oyuncunun ObjectPickup script'ini bul ve slotlarý ona kaydettir.
        ObjectPickup playerPickupScript = FindObjectOfType<ObjectPickup>();
        if (playerPickupScript != null)
        {
            playerPickupScript.RegisterSlots(itemSlots);
        }
        else
        {
            Debug.LogError("Sahnede ObjectPickup script'i bulunamadý!", this.gameObject);
        }
    }

    // Bu raf sahneden silindiðinde çalýþýr (isteðe baðlý ama iyi bir pratik)
    void OnDestroy()
    {
        // FindObjectOfType, obje yok olurken null dönebilir, bu yüzden null kontrolü önemli.
        ObjectPickup playerPickupScript = FindObjectOfType<ObjectPickup>();
        if (playerPickupScript != null)
        {
            playerPickupScript.UnregisterSlots(itemSlots);
        }
    }
}