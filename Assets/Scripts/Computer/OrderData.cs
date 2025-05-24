// OrderData.cs (veya ilgili yöneticinin içinde)
using System.Collections.Generic;
using UnityEngine; // Eðer oyun içi zaman damgalarý için Time.time kullanacaksan

// Bir sipariþteki her bir ürün kalemini temsil eder
[System.Serializable] // Inspector'da görülebilmesi için
public class OrderItemDetail
{
    public InGameMarket.MarketProduct productDefinition; // Ürünün InGameMarket'teki tanýmýna referans
    public int quantity;
    public int unitSellPriceAtOrderTime; // Sipariþ anýndaki birim satýþ fiyatý (oyuncu belirlemiþ olmalý)

    public OrderItemDetail(InGameMarket.MarketProduct productDef, int qty, int price)
    {
        productDefinition = productDef;
        quantity = qty;
        unitSellPriceAtOrderTime = price;
    }
}

// Sipariþin durumunu belirten enum
public enum OrderStatus
{
    Yeni,           // Oyuncu tarafýndan henüz kabul edilmemiþ/görülmemiþ
    Hazýrlanýyor,   // Oyuncu kabul etti, ürünleri topluyor/paketlemeye baþladý
    Paketlendi,     // Paketleme tamamlandý, kargoya verilmeye hazýr
    Kargoda,        // Kargo þubesine teslim edildi
    TeslimEdildi,   // Müþteriye ulaþtý (ve ödeme alýndý)
    ÝptalEdildi     // Oyuncu veya sistem tarafýndan iptal edildi
}

// Sipariþin tipini belirten enum (GDD'den)
public enum OrderType
{
    Standart,
    Express,
    AyniGun // GDD'de "Ayný Gün" olarak geçiyordu
}

// Ana sipariþ verisini tutan sýnýf
[System.Serializable]
public class OrderData
{
    public string orderID;
    public List<OrderItemDetail> itemsInOrder;
    public string customerName;
    // public string deliveryAddress; // Demo için þimdilik basitleþtirilebilir
    public float orderTimestamp;       // Oyun içi sipariþ verilme zamaný (TimeManager'dan alýnabilir)
    public float dueTimestamp;         // Oyun içi son teslim tarihi/saati
    public OrderType orderType;
    public OrderStatus status;
    public int totalOrderValue;        // Sipariþin toplam deðeri (oyuncunun satýþ fiyatlarý üzerinden)
    public float timeMultiplierAtOrderCreation; // Sipariþ oluþturulduðundaki TimeManager.timeMultiplier deðeri (dueTimestamp hesaplamasý için)


    public OrderData()
    {
        orderID = System.Guid.NewGuid().ToString().Substring(0, 8); // Basit bir benzersiz ID
        itemsInOrder = new List<OrderItemDetail>();
        status = OrderStatus.Yeni;
        customerName = "Müþteri " + Random.Range(1000, 9999); // Rastgele müþteri adý
        // Diðer baþlangýç deðerleri atanabilir
    }
}