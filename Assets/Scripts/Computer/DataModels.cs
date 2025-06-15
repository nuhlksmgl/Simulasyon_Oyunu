using UnityEngine;
using System.Collections.Generic;
using System;

// Kargo kutularýnýn özelliklerini tutar.
[System.Serializable]
public class CargoBoxData
{
    public string boxName;
    public GameObject boxPrefab;
    public int largeItemCapacity;
    public int smallItemCapacity;
}

// Alýþveriþ sepetindeki kargo seçeneklerini tutar.
[System.Serializable]
public class CargoOption
{
    public string displayName;
    public float price;
    public float deliveryTimeMultiplier;
}

// Pazarda satýlan veya sipariþ edilen tek bir ürünü temsil eder.
[System.Serializable]
public class MarketProduct
{
    public string productName;
    public Sprite productImage;
    public int price;
    public bool isLarge;
    public GameObject productPrefab;

    [Tooltip("Ýþaretliyse, ürün bir kutu içinde deðil, doðrudan kendisi olarak teslim edilir.")]
    public bool isDirectDelivery;

    [Tooltip("Ýþaretliyse, ürün satýn alýndýðýnda anýnda etki eder, kargoyla gelmez.")]
    public bool isInstantPurchase;

    [Tooltip("Bu ürün bir kerelik bir satýn alým mý? (Örn: Dükkan Geniþletme)")]
    public bool isOneTimePurchase = false;
    [HideInInspector] public bool isPurchased = false;

    [Tooltip("Bu üründen kaç adet satýn alýndýðýný sayar.")]
    [HideInInspector] public int purchaseCount = 0;

    [Header("Piyasa Dinamikleri")]
    public float baseMarketPrice;
    public float currentAverageMarketPrice;
    [HideInInspector] public int priceTrendStreak = 0;

    [Header("Oyuncu Verisi")]
    public int physicalStock;
    [HideInInspector] public int inTransitStock = 0;
}

// Ürünleri gruplamak için kullanýlýr.
[System.Serializable]
public class Category
{
    public string categoryName;
    public Sprite categoryIcon;
    public int categoryLicenseCost;
    public bool isUnlocked = false;
    public List<MarketProduct> productsInCategory;
}

// Müþteri sipariþinin durumunu belirtir.
public enum OrderStatus { Yeni, Hazirlaniyor, Completed, Failed }

// Müþteri sipariþinin türünü belirtir.
public enum OrderType { Standart, Express, AyniGun }

// Bir sipariþ içindeki tek bir ürün kaleminin detaylarýný tutar.
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

// Tek bir müþteri sipariþinin tüm verilerini içeren ana sýnýf.
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
            {
                foreach (var item in itemsInOrder)
                {
                    total += item.pricePerItem * item.quantity;
                }
            }
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