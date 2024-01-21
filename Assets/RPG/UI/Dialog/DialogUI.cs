﻿using RPG.Dialogs;
using UnityEngine;
using UnityEngine.UIElements;

namespace RPG.UI.Dialog {
    public class DialogUI : MonoBehaviour {

        [SerializeField] private UIDocument _document;
        
        private VisualElement _root;
        private PlayerConversant _playerConversant;
        private Label _currentConversant;
        private Label _dialogText;
        private Box _playerResponses;

        private Button _closeButton;

        private void Awake() {
            _playerConversant = GameObject.FindWithTag("Player").GetComponent<PlayerConversant>();
        }

        private void OnEnable() {
            _root = _document.rootVisualElement;
            _currentConversant = _root.Q<Label>("conversant");
            _dialogText = _root.Q<Label>("dialog");
            _playerResponses = _root.Q<Box>("choices");
            _playerConversant.OnUpdate += UpdateUI;
            UpdateUI();
            print(_currentConversant.text);

            _closeButton = _root.Q<Button>(className: "dialog__button_close");
            _closeButton.clicked += CloseDialog;
        }
        private void CloseDialog() {
            print("quit...");
            _playerConversant.Quit();
        }

        private void UpdateUI() {
            print("checking...");
            gameObject.SetActive(_playerConversant.IsActive);
            if (!_playerConversant.IsActive) return;
            print("Active");
            _currentConversant.text = _playerConversant.CurrentConversantName;
            CleanResponses();
            if (_playerConversant.IsChoosing) {
                print("building...");
                BuildChooseList();
            }
            else {
                _dialogText.text = _playerConversant.GetCurrentText();
                print(_playerConversant.GetCurrentText() + " " + _dialogText.text);
                if (!_playerConversant.HasNext()) return;
                var button = new Button(() => {
                    print("issue"); _playerConversant.Next();}) {
                    text = "Next"
                };
                _playerResponses.Add(button);
            }
        }
        private void CleanResponses() {
            print("Clearing...");
            _playerResponses.Clear();
        }
        private void BuildChooseList() {
            _playerResponses.Clear();
            foreach (var choice in _playerConversant.GetChoices()) {
                var button = new Button(() => { print("issue"); _playerConversant.SelectChoice(choice);}) {
                    text = choice.Text
                };
                _playerResponses.Add(button);
            }
        }
    }
}
