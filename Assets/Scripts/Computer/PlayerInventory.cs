using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    [System.Serializable]
    public class InventoryItem
    {
        public string productName; // �r�n ad�
        public GameObject productPrefab; // �r�n�n prefab'�
        public int quantity; // �r�n�n sto�u
        public int price; // �r�n�n fiyat� (al�n�rken veya sat�l�rken)

        public InventoryItem(string name, GameObject prefab, int initialQuantity, int price)
        {
            productName = name;
            productPrefab = prefab;
            quantity = initialQuantity;
            this.price = price;
        }
    }

    public List<InventoryItem> inventoryItems = new List<InventoryItem>();

    public void AddItem(string productName, GameObject productPrefab, int price)
    {
        var item = inventoryItems.Find(i => i.productName == productName);
        if (item != null)
        {
            item.quantity++;
        }
        else
        {
            inventoryItems.Add(new InventoryItem(productName, productPrefab, 1, price));
        }
    }

    public void RemoveItem(string productName)
    {
        var item = inventoryItems.Find(i => i.productName == productName);
        if (item != null)
        {
            item.quantity--;
            if (item.quantity <= 0)
            {
                inventoryItems.Remove(item);
            }
        }
    }
}
