using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Carryable : MonoBehaviour
{
    private Rigidbody rb;
    private void Awake() { rb = GetComponent<Rigidbody>(); }
    public void OnPickedUp() { if (rb != null) rb.isKinematic = true; }
    public void OnDropped() { if (rb != null) rb.isKinematic = false; }
}