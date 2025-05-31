
using System.Collections.Generic;
using UnityEngine;

// OrderData.cs içindeki OrderItemDetail sýnýfý

[System.Serializable]
public class OrderItemDetail
{
    public InGameMarket.MarketProduct productDefinition;
    public int quantity;
    public int unitSellPriceAtOrderTime;
    public int marketPriceAtOrderTime;   // <--- YENÝ EKLENEN ALAN

    // Constructor'ý da güncelle
    public OrderItemDetail(InGameMarket.MarketProduct productDef, int qty, int sellPrice, int marketPrice) // marketPrice parametresi eklendi
    {
        productDefinition = productDef;
        quantity = qty;
        unitSellPriceAtOrderTime = sellPrice;
        marketPriceAtOrderTime = marketPrice; // <--- YENÝ ATAMA
    }
}

public enum OrderStatus
{
    Yeni,
    Hazirlaniyor,
    Paketlendi,
    Kargoda,
    TeslimEdildi,
    ÝptalEdildi
}

public enum OrderType
{
    Standart,
    Express,
    AyniGun
}

[System.Serializable]
public class OrderData
{
    public string orderID;
    public List<OrderItemDetail> itemsInOrder;
    public string customerName;
    public float orderTimestamp;
    public float dueTimestamp;
    public OrderType orderType;
    public OrderStatus status;
    public int totalOrderValue;
    public float timeMultiplierAtOrderCreation;

    public OrderData()
    {
        orderID = System.Guid.NewGuid().ToString().Substring(0, 8);
        itemsInOrder = new List<OrderItemDetail>();
        status = OrderStatus.Yeni;
        // customerName artýk InitializeCustomer() ile atanýyor
    }

    public void InitializeCustomer()
    {
        customerName = "Müþteri " + UnityEngine.Random.Range(1000, 9999);
    }
}
