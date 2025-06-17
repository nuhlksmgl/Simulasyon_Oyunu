using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class DeliveryManager : MonoBehaviour
{
    public static DeliveryManager Instance { get; private set; }

    [Header("Teslimat Ayarlarý")]
    [SerializeField] private BoxCollider deliveryArea;
    [SerializeField] private List<CargoBoxData> boxTypes;
    [SerializeField] private Transform[] shelfSpawnPoints;
    private int nextShelfSpawnIndex = 0;

    [Header("Genel Spawn Ayarlarý")]
    [SerializeField] private int maxSpawnAttempts = 20;

    private class ActiveDelivery { public float DeliveryTimestamp; public List<PackedBox> PackedBoxes; }
    private class PackedBox { public GameObject BoxPrefab; public List<MarketProduct> ItemsInBox; }
    private List<ActiveDelivery> activeDeliveries = new List<ActiveDelivery>();

    private class ActiveDirectDelivery { public float DeliveryTimestamp; public GameObject ProductPrefab; }
    private List<ActiveDirectDelivery> activeDirectDeliveries = new List<ActiveDirectDelivery>();

    void Awake() { Instance = this; }

    void Update()
    {
        CheckForDueBoxedDeliveries();
        CheckForDueDirectDeliveries();
    }

    public void ScheduleNewDelivery(List<ShoppingCart.CartItem> items, CargoOption cargo)
    {
        float baseDeliveryMinutes = 24 * 60;
        float finalDeliveryMinutes = baseDeliveryMinutes * cargo.deliveryTimeMultiplier;
        float deliveryTimestamp = TimeManager.Instance.GetTotalMinutesPassedInGame() + finalDeliveryMinutes;
        List<PackedBox> packedBoxes = PackItemsIntoBoxes(items);
        if (packedBoxes.Count > 0)
        {
            activeDeliveries.Add(new ActiveDelivery { DeliveryTimestamp = deliveryTimestamp, PackedBoxes = packedBoxes });
        }
    }

    public void ScheduleDirectDelivery(MarketProduct product, CargoOption cargo)
    {
        float baseDeliveryMinutes = 24 * 60;
        float finalDeliveryMinutes = baseDeliveryMinutes * cargo.deliveryTimeMultiplier;
        float deliveryTimestamp = TimeManager.Instance.GetTotalMinutesPassedInGame() + finalDeliveryMinutes;
        activeDirectDeliveries.Add(new ActiveDirectDelivery
        {
            DeliveryTimestamp = deliveryTimestamp,
            ProductPrefab = product.productPrefab
        });
    }

    private List<PackedBox> PackItemsIntoBoxes(List<ShoppingCart.CartItem> items)
    {
        var resultingPackedBoxes = new List<PackedBox>();
        var allItems = new List<MarketProduct>();
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

    void CheckForDueBoxedDeliveries()
    {
        if (activeDeliveries.Count == 0) return;
        float currentTime = TimeManager.Instance.GetTotalMinutesPassedInGame();
        var dueDeliveries = activeDeliveries.Where(d => currentTime >= d.DeliveryTimestamp).ToList();
        foreach (var delivery in dueDeliveries)
        {
            SpawnBoxes(delivery.PackedBoxes);
            activeDeliveries.Remove(delivery);
        }
    }

    void CheckForDueDirectDeliveries()
    {
        if (activeDirectDeliveries.Count == 0) return;
        float currentTime = TimeManager.Instance.GetTotalMinutesPassedInGame();
        var dueDeliveries = activeDirectDeliveries.Where(d => currentTime >= d.DeliveryTimestamp).ToList();
        foreach (var delivery in dueDeliveries)
        {
            SpawnDirectProduct(delivery.ProductPrefab);
            activeDirectDeliveries.Remove(delivery);
        }
    }

    void SpawnBoxes(List<PackedBox> packedBoxes)
    {
        if (deliveryArea == null) { Debug.LogError("DeliveryManager: Teslimat Alaný atanmamýþ!"); return; }

        try
        {
            deliveryArea.enabled = false;
            foreach (var packedBox in packedBoxes)
            {
                GameObject boxInstance = SpawnSingleObjectInArea(packedBox.BoxPrefab);
                if (boxInstance != null)
                {
                    var cargoBoxScript = boxInstance.GetComponent<CargoBox>();
                    if (cargoBoxScript != null)
                    {
                        cargoBoxScript.InitializeBox(packedBox.ItemsInBox);
                        foreach (var product in packedBox.ItemsInBox)
                        {
                            if (product.inTransitStock > 0) product.inTransitStock--;
                            product.physicalStock++;
                        }
                    }
                }
            }
        }
        finally
        {
            deliveryArea.enabled = true;
        }
    }

    void SpawnDirectProduct(GameObject productPrefab)
    {
        if (shelfSpawnPoints == null || shelfSpawnPoints.Length == 0)
        {
            Debug.LogError("HATA: Raf için spawn noktasý (Shelf Spawn Points) atanmamýþ!");
            return;
        }
        Transform spawnPoint = shelfSpawnPoints[nextShelfSpawnIndex];
        GameObject instance = Instantiate(productPrefab, spawnPoint.position, spawnPoint.rotation);
        Product productScript = instance.GetComponent<Product>();
        if (productScript != null && productScript.productDefinition != null)
        {
            if (productScript.productDefinition.inTransitStock > 0) productScript.productDefinition.inTransitStock--;
            productScript.productDefinition.physicalStock++;
        }
        nextShelfSpawnIndex = (nextShelfSpawnIndex + 1) % shelfSpawnPoints.Length;
    }

    GameObject SpawnSingleObjectInArea(GameObject objectToSpawn)
    {
        Collider objectCollider = objectToSpawn.GetComponent<Collider>();
        if (objectCollider == null) { Debug.LogError($"{objectToSpawn.name} prefabýnda Collider yok!"); return null; }

        Bounds areaBounds = deliveryArea.bounds;
        Vector3 objectHalfExtents = objectCollider.bounds.extents;
        Vector3 spawnPosition = Vector3.zero;
        int attempts = 0;

        while (attempts < maxSpawnAttempts)
        {
            float randomX = Random.Range(areaBounds.min.x + objectHalfExtents.x, areaBounds.max.x - objectHalfExtents.x);
            float randomZ = Random.Range(areaBounds.min.z + objectHalfExtents.z, areaBounds.max.z - objectHalfExtents.z);
            Vector3 rayStartPoint = new Vector3(randomX, areaBounds.max.y, randomZ);

            if (Physics.Raycast(rayStartPoint, Vector3.down, out RaycastHit hit, areaBounds.size.y))
            {
                spawnPosition = new Vector3(randomX, hit.point.y + objectHalfExtents.y, randomZ);
            }
            else
            {
                spawnPosition = new Vector3(randomX, areaBounds.min.y + objectHalfExtents.y, randomZ);
            }

            if (Physics.OverlapBox(spawnPosition, objectHalfExtents, Quaternion.identity).Length == 0)
            {
                return Instantiate(objectToSpawn, spawnPosition, Quaternion.identity);
            }
            attempts++;
        }
        Debug.LogWarning($"Teslimat alaný çok dolu, {objectToSpawn.name} için uygun bir yer bulunamadý!");
        return null;
    }
}