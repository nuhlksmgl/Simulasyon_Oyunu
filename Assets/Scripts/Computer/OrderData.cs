using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class OrderItemDetail
{
    public InGameMarket.MarketProduct productDefinition;
    public int quantity;
    public int unitSellPriceAtOrderTime;
    public int marketPriceAtOrderTime;

    public OrderItemDetail(InGameMarket.MarketProduct productDef, int qty, int sellPrice, int marketPrice)
    {
        productDefinition = productDef;
        quantity = qty;
        unitSellPriceAtOrderTime = sellPrice;
        marketPriceAtOrderTime = marketPrice;
    }
}

public enum OrderStatus
{
    Yeni,
    Hazirlaniyor,
    Paketlendi,
    Kargoda,
    TeslimEdildi,
    IptalEdildi
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
        customerName = "";
        orderTimestamp = 0f;
        dueTimestamp = 0f;
        orderType = OrderType.Standart;
        totalOrderValue = 0;
        timeMultiplierAtOrderCreation = 1f;
    }

    public void InitializeCustomer()
    {
        customerName = "Müþteri " + UnityEngine.Random.Range(1000, 9999).ToString();
        Debug.Log($"OrderData: Müþteri atandý - {customerName} için Sipariþ ID: {orderID}");
    }
}