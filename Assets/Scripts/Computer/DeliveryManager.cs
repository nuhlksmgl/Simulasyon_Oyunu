using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class DeliveryManager : MonoBehaviour
{
    public static DeliveryManager Instance { get; private set; }

    [Header("Teslimat Ayarlarý")]
    [SerializeField] private BoxCollider deliveryArea;
    [SerializeField] private List<CargoBoxData> boxTypes;
    [SerializeField] private float minDistanceBetweenBoxes = 1f;
    [SerializeField] private int maxSpawnAttempts = 20;

    private class ActiveDelivery
    {
        public float DeliveryTimestamp;
        public List<PackedBox> PackedBoxes;
    }
    private class PackedBox
    {
        public GameObject BoxPrefab;
        public List<MarketProduct> ItemsInBox;
    }
    private List<ActiveDelivery> activeDeliveries = new List<ActiveDelivery>();

    void Awake() { Instance = this; }

    void Update() { CheckForDueDeliveries(); }

    public void ScheduleNewDelivery(List<ShoppingCart.CartItem> items, CargoOption cargo)
    {
        float baseDeliveryMinutes = 24 * 60;
        float finalDeliveryMinutes = baseDeliveryMinutes * cargo.deliveryTimeMultiplier;
        float deliveryTimestamp = TimeManager.Instance.GetTotalMinutesPassedInGame() + finalDeliveryMinutes;

        List<PackedBox> packedBoxes = PackItemsIntoBoxes(items);

        if (packedBoxes.Count > 0)
        {
            activeDeliveries.Add(new ActiveDelivery
            {
                DeliveryTimestamp = deliveryTimestamp,
                PackedBoxes = packedBoxes
            });

            // --- KONTROL 2 ---
            Debug.Log($"2. GÖREV ALINDI: {packedBoxes.Count} kutu, {deliveryTimestamp}. dakikada teslim edilmek üzere planlandý.");
        }
    }

    private List<PackedBox> PackItemsIntoBoxes(List<ShoppingCart.CartItem> items)
    {
        var resultingPackedBoxes = new List<PackedBox>();
        List<MarketProduct> allItems = new List<MarketProduct>();
        foreach (var cartItem in items) { for (int i = 0; i < cartItem.Quantity; i++) { allItems.Add(cartItem.Product); } }

        var largeItems = allItems.Where(p => p.isLarge).ToList();
        var smallItems = allItems.Where(p => !p.isLarge).ToList();
        var largeBoxTypes = boxTypes.Where(b => b.largeItemCapacity > 0).OrderByDescending(b => b.largeItemCapacity).ToList();
        var smallBoxTypes = boxTypes.Where(b => b.smallItemCapacity > 0).OrderByDescending(b => b.smallItemCapacity).ToList();

        while (largeItems.Count > 0)
        {
            var bestBox = largeBoxTypes.FirstOrDefault(b => largeItems.Count >= b.largeItemCapacity) ?? largeBoxTypes.LastOrDefault();
            if (bestBox == null) break;
            int itemsToPackCount = Mathf.Min(largeItems.Count, bestBox.largeItemCapacity);
            List<MarketProduct> itemsForThisBox = largeItems.GetRange(0, itemsToPackCount);
            largeItems.RemoveRange(0, itemsToPackCount);
            resultingPackedBoxes.Add(new PackedBox { BoxPrefab = bestBox.boxPrefab, ItemsInBox = itemsForThisBox });
        }
        while (smallItems.Count > 0)
        {
            var bestBox = smallBoxTypes.FirstOrDefault(b => smallItems.Count >= b.smallItemCapacity) ?? smallBoxTypes.LastOrDefault();
            if (bestBox == null) break;
            int itemsToPackCount = Mathf.Min(smallItems.Count, bestBox.smallItemCapacity);
            List<MarketProduct> itemsForThisBox = smallItems.GetRange(0, itemsToPackCount);
            smallItems.RemoveRange(0, itemsToPackCount);
            resultingPackedBoxes.Add(new PackedBox { BoxPrefab = bestBox.boxPrefab, ItemsInBox = itemsForThisBox });
        }
        return resultingPackedBoxes;
    }

    void CheckForDueDeliveries()
    {
        if (activeDeliveries.Count == 0) return;
        float currentTime = TimeManager.Instance.GetTotalMinutesPassedInGame();
        var dueDeliveries = activeDeliveries.Where(d => currentTime >= d.DeliveryTimestamp).ToList();

        foreach (var delivery in dueDeliveries)
        {
            // --- KONTROL 3 ---
            Debug.Log($"3. TESLÝMAT ZAMANI GELDÝ! {delivery.PackedBoxes.Count} kutu spawn edilecek.");

            SpawnBoxes(delivery.PackedBoxes);
            activeDeliveries.Remove(delivery);
        }
    }

    void SpawnBoxes(List<PackedBox> packedBoxes)
    {
        if (deliveryArea == null) { return; }
        Bounds areaBounds = deliveryArea.bounds;

        foreach (var packedBox in packedBoxes)
        {
            Vector3 spawnPosition = Vector3.zero;
            int attempts = 0;
            while (attempts < maxSpawnAttempts)
            {
                float randomX = Random.Range(areaBounds.min.x, areaBounds.max.x);
                float randomZ = Random.Range(areaBounds.min.z, areaBounds.max.z);
                spawnPosition = new Vector3(randomX, areaBounds.min.y, randomZ);
                if (!Physics.CheckSphere(spawnPosition, minDistanceBetweenBoxes)) break;
                attempts++;
            }

            // --- KONTROL 4 ---
            Debug.Log($"4. SPAWN EDÝLÝYOR: {packedBox.BoxPrefab.name} prefabý, {spawnPosition} konumunda oluþturuluyor.");

            GameObject boxInstance = Instantiate(packedBox.BoxPrefab, spawnPosition, Quaternion.identity);
            boxInstance.GetComponent<CargoBox>()?.InitializeBox(packedBox.ItemsInBox);
        }
    }
}