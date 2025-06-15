using UnityEngine;
using System.Collections.Generic;
using System;

//##################################################################
//##### OYUNCU PAZARI VE ENVANTER ÝÇÝN VERÝ MODELLERÝ
//##################################################################

/// <summary>
/// Kargo kutularýnýn özelliklerini tutar.
/// </summary>
[System.Serializable]
public class CargoBoxData
{
    public string boxName;
    public GameObject boxPrefab;
    public int largeItemCapacity;
    public int smallItemCapacity;
}

/// <summary>
/// Alýþveriþ sepetindeki kargo seçeneklerini tutar.
/// </summary>
[System.Serializable]
public class CargoOption
{
    public string displayName;
    public float price;
    public float deliveryTimeMultiplier;
}

/// <summary>
/// Pazarda satýlan veya sipariþ edilen tek bir ürünü temsil eder.
/// </summary>
[System.Serializable]
public class MarketProduct
{
    public string productName;
    public Sprite productImage;
    public int price;

    // DÜZELTME: Diðer script'lerle uyumlu olmasý için 'isLargeItem' -> 'isLarge' olarak deðiþtirildi.
    public bool isLarge;

    public GameObject productPrefab;

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
    public int quantity;
}

/// <summary>
/// Ürünleri gruplamak için kullanýlýr. Lisanslar bu seviyede yönetilir.
/// </summary>
[System.Serializable]
public class Category
{
    public string categoryName;
    public Sprite categoryIcon;
    public int categoryLicenseCost;
    public bool isUnlocked = false;
    public List<MarketProduct> productsInCategory;
}


//##################################################################
//##### MÜÞTERÝ SÝPARÝÞLERÝ ÝÇÝN VERÝ MODELLERÝ
//##################################################################

/// <summary>
/// Müþteri sipariþinin durumunu belirtir.
/// </summary>
public enum OrderStatus { Yeni, Hazirlaniyor, Completed, Failed }

/// <summary>
/// Müþteri sipariþinin türünü belirtir.
/// </summary>
public enum OrderType { Standart, Express, AyniGun }

/// <summary>
/// Bir sipariþ içindeki tek bir ürün kaleminin detaylarýný tutar.
/// </summary>
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

/// <summary>
/// Tek bir müþteri sipariþinin tüm verilerini içeren ana sýnýf.
/// </summary>
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