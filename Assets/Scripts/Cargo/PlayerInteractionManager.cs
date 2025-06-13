using UnityEngine;
using UnityEngine.UI;

public class PlayerInteractionManager : MonoBehaviour
{
    [Header("Kontrol Edilecek Scriptler")]
    [SerializeField] private ObjectPickup objectPickupScript;
    [SerializeField] private ObjectPlacementController objectPlacementScript;

    [Header("UI Bildirimleri (Opsiyonel)")]
    [SerializeField] private GameObject editModeIndicator;

    private bool isEditMode = false;

    void Start()
    {
        EnterNormalMode();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (objectPickupScript.IsHoldingAnything())
            {
                Debug.LogWarning("Eliniz doluyken mod deðiþtiremezsiniz!");
                return;
            }

            isEditMode = !isEditMode;

            if (isEditMode)
            {
                EnterEditMode();
            }
            else
            {
                EnterNormalMode();
            }
        }
    }

    void EnterEditMode()
    {
        isEditMode = true;
        objectPickupScript.enabled = false;
        objectPlacementScript.enabled = true;

        if (editModeIndicator != null) editModeIndicator.SetActive(true);
        Debug.Log("EDIT MODE AKTÝF");
    }

    void EnterNormalMode()
    {
        isEditMode = false;
        objectPickupScript.enabled = true;
        objectPlacementScript.enabled = false;

        if (editModeIndicator != null) editModeIndicator.SetActive(false);
        Debug.Log("NORMAL MODE AKTÝF");
    }
}