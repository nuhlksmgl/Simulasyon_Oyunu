using UnityEngine;

public class PackingStation : MonoBehaviour
{
    public GameObject[] smallBox1Prefabs;
    public GameObject[] smallBox2Prefabs;
    public GameObject[] smallBox3to4Prefabs;

    public GameObject[] largeBox1Prefabs;
    public GameObject[] largeBox2Prefabs;
    public GameObject[] largeBox3to4Prefabs;

    public Transform spawnPoint;

    private GameObject currentBox;

    public void SpawnCargoBoxForOrder(OrderData order)
    {
        if (currentBox != null)
            Destroy(currentBox);

        int totalQty = 0;
        bool containsLarge = false;

        foreach (var item in order.itemsInOrder)
        {
            totalQty += item.quantity;
            if (item.productDefinition.isLarge)
                containsLarge = true;
        }

        GameObject[] chosenArray = null;

        if (containsLarge)
        {
            if (totalQty == 1)
                chosenArray = largeBox1Prefabs;
            else if (totalQty == 2)
                chosenArray = largeBox2Prefabs;
            else
                chosenArray = largeBox3to4Prefabs;
        }
        else
        {
            if (totalQty == 1)
                chosenArray = smallBox1Prefabs;
            else if (totalQty == 2)
                chosenArray = smallBox2Prefabs;
            else
                chosenArray = smallBox3to4Prefabs;
        }

        if (chosenArray == null || chosenArray.Length == 0)
        {
            Debug.LogError("Uygun kutu prefabı atanmadı!");
            return;
        }

        GameObject selectedPrefab = chosenArray[Random.Range(0, chosenArray.Length)];
        Quaternion spawnRotation = spawnPoint.rotation * Quaternion.Euler(-90f, 0f, 0f);
        currentBox = Instantiate(selectedPrefab, spawnPoint.position, spawnRotation);


        CargoBox cargoBox = currentBox.GetComponent<CargoBox>();
        if (cargoBox != null)
        {
            cargoBox.AssignOrder(order); // ❗ ürünler otomatik olarak konmaz
        }
    }
}
