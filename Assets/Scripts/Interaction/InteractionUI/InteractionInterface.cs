using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface Interactable
{
    string GetPromptMessage(); // UI'da görünecek mesaj
    void OnInteract();         // Etkileşim gerçekleştiğinde çağrılacak
}