using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class MainMenu_Events : MonoBehaviour
{
    private UIDocument _document;

    private Button _button;

    private List<Button> _menuButtons = new List<Button>();

    private AudioSource _audioClick;


    private void Awake()
    {
       
        _audioClick = GetComponent<AudioSource>();

        _document = GetComponent<UIDocument>();

        _button = _document.rootVisualElement.Q("ButtonA") as Button;
        _button.RegisterCallback<ClickEvent>(OnButtonAClick);

        _menuButtons = _document.rootVisualElement.Query<Button>().ToList();
        for (int i = 0; i < _menuButtons.Count; i++)
        {
            _menuButtons[i].RegisterCallback<ClickEvent>(OnAllButtonsClick);
            _menuButtons[i].RegisterCallback<MouseEnterEvent>(OnAllButtonsMouseEnter);
        }
    }


    private void OnDisable()
    {
        _button.UnregisterCallback<ClickEvent>(OnButtonAClick);

        for (int i = 0; i < _menuButtons.Count; i++)
        {
            _menuButtons[i].RegisterCallback<ClickEvent>(OnAllButtonsClick);
        }
    }


    private void OnButtonAClick(ClickEvent evt)
    {
        Debug.Log("You pressed Button A!");
    }

    private void OnAllButtonsMouseEnter(MouseEnterEvent evt)
    {
        Debug.Log("You Hovered over a button!");
        _audioClick.Play();
    }

    private void OnAllButtonsClick(ClickEvent evt)
    {
        Debug.Log("You pressed a button!");
        _audioClick.Play();
    }
}