using UnityEngine;
using System.Collections.Generic;
using System;

[System.Serializable]
public class CargoBoxData
{
    public string boxName;
    public GameObject boxPrefab;
    public int largeItemCapacity;
    public int smallItemCapacity;
}

[System.Serializable]
public class CargoOption
{
    public string displayName;
    public float price;
    public float deliveryTimeMultiplier;
}

[System.Serializable]
public class MarketProduct
{
    public string productName;
    public Sprite productImage;
    public int price;
    public bool isLarge;
    public GameObject productPrefab;
    public bool isDirectDelivery;
    public bool isInstantPurchase;
    public bool isOneTimePurchase = false;
    [HideInInspector] public bool isPurchased = false;
    [HideInInspector] public int purchaseCount = 0;

    [Header("Piyasa Dinamikleri")]
    public float baseMarketPrice;
    public float currentAverageMarketPrice;
    [HideInInspector] public int priceTrendStreak = 0;

    [Header("Oyuncu Verisi")]
    public int physicalStock;
    [HideInInspector] public int inTransitStock = 0;

    // YENÝ EKLENDÝ: Oyuncunun bu ürünü SellPanel'de satýþa koyup koymadýðýný belirtir.
    [HideInInspector] public bool isListedForSale = false;
}

[System.Serializable]
public class Category
{
    public string categoryName;
    public Sprite categoryIcon;
    public int categoryLicenseCost;
    public bool isUnlocked = false;
    public List<MarketProduct> productsInCategory;
}

public enum OrderStatus { Yeni, Hazirlaniyor, Completed, Failed, Paketlendi, Kargoda, TeslimEdildi, IptalEdildi }
public enum OrderType { Standart, Express, AyniGun }

[System.Serializable]
public class OrderItemDetail
{
    public MarketProduct productDefinition;
    public int quantity;
    public int pricePerItem;
    public int marketAverageOnOrder;
    public int unitSellPriceAtOrderTime;

    public OrderItemDetail(MarketProduct product, int qty, int price, int marketAvg)
    {
        this.productDefinition = product;
        this.quantity = qty;
        this.pricePerItem = price;
        this.marketAverageOnOrder = marketAvg;
        this.unitSellPriceAtOrderTime = price;
    }
}

[System.Serializable]
public class OrderData
{
    public string orderID;
    public string customerName;
    public List<OrderItemDetail> itemsInOrder;
    public OrderStatus status;
    public OrderType orderType;
    public DateTime orderTimestamp;
    public float timeLimit;

    public float totalOrderValue
    {
        get
        {
            float total = 0;
            if (itemsInOrder != null)
                foreach (var item in itemsInOrder)
                    total += item.pricePerItem * item.quantity;
            return total;
        }
    }

    public OrderData()
    {
        orderID = Guid.NewGuid().ToString().Substring(0, 8).ToUpper();
        itemsInOrder = new List<OrderItemDetail>();
        status = OrderStatus.Yeni;
        orderTimestamp = DateTime.Now;
        orderType = OrderType.Standart;
    }

    public void InitializeCustomer()
    {
        this.customerName = "Müþteri " + new System.Random().Next(1000, 9999);
    }
}