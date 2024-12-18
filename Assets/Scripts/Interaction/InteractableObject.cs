using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractableObject : MonoBehaviour, Interactable
{
    [SerializeField] private string promptMessage = "Press E to interact";
    [SerializeField] private bool canPickup = true;
    [SerializeField] private Color highlightColor = Color.yellow;

    private Material material;
    private Color originalColor;
    private bool isHighlighted = false;

    // Bu özelliği virtual olarak işaretleyin
    public virtual bool CanPickup => canPickup; // Bu satırı virtual olarak işaretleyin

    private void Start()
    {
        // Objenin material'ini al
        material = GetComponent<Renderer>().material;
        originalColor = material.color;
    }

    public virtual string GetPromptMessage()  // Burayı virtual olarak işaretle
    {
        return promptMessage;
    }

    public virtual void OnInteract()  // Burayı virtual olarak işaretle
    {
        Debug.Log($"Interacted with {gameObject.name}");
    }

    public void Highlight(bool state)
    {
        if (state == isHighlighted) return;

        material.color = state ? highlightColor : originalColor;
        isHighlighted = state;
    }
}
