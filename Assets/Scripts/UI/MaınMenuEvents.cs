using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
public class MaınMenuEvents : MonoBehaviour
{
    private UIDocument document;
    private Button button;
    private List<Button> menuButtons= new List<Button>();
    private AudioSource audioSource;


    private void Awake()
    {
        document = GetComponent<UIDocument>();
        button = document.rootVisualElement.Q("StartGameButton") as Button;
        button.RegisterCallback<ClickEvent>(OnPlayGameClick);
        menuButtons = document.rootVisualElement.Query<Button>().ToList();

        for (int i = 0; i < menuButtons.Count; i++)
        {
            menuButtons[i].RegisterCallback<ClickEvent>(OnAllButtonsClick);
        }
    }

    private void OnDisable()
    {
        button.UnregisterCallback<ClickEvent>(OnPlayGameClick);

        for (int i = 0; i < menuButtons.Count; i++)
        {
            menuButtons[i].UnregisterCallback<ClickEvent>(OnAllButtonsClick);
        }
    }


    private void OnPlayGameClick(ClickEvent evt)
    {
        Debug.Log("You pressed the Start Button");

    }

    private void OnAllButtonsClick(ClickEvent evt)
    {

        audioSource.Play();

    }
}
