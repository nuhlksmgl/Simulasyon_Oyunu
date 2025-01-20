using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    [System.Serializable]
    public class InventoryItem
    {
        public string productName; // Ürün adý
        public GameObject productPrefab; // Ürünün prefab'ý
        public int quantity; // Ürünün stoðu
        public int price; // Ürünün fiyatý (alýnýrken veya satýlýrken)

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
