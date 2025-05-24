using UnityEngine;

public class ActiveOrderManager : MonoBehaviour
{
    public static ActiveOrderManager Instance;
    public OrderData activeOrder;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void SetActiveOrder(OrderData order)
    {
        activeOrder = order;
        Debug.Log($"Aktif sipariþ ayarlandý: {order.orderID}");
    }

    public void ClearActiveOrder()
    {
        activeOrder = null;
    }
}
