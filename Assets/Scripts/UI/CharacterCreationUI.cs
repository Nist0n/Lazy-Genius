using System;
using System.Collections.Generic;
using Abstractions.Characters;
using Player;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class CharacterCreationUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TMP_InputField characterNameInput;
        [SerializeField] private Button createButton;
        [SerializeField] private Button cancelButton;
        
        [Header("Class Selection")]
        [SerializeField] private Transform classButtonContainer;
        [SerializeField] private GameObject classButtonPrefab;
        [SerializeField] private TextMeshProUGUI classNameText;
        [SerializeField] private TextMeshProUGUI classDescriptionText;
        [SerializeField] private Image classIconImage;
        
        [Header("Validation")]
        [SerializeField] private TextMeshProUGUI errorMessageText;
        [SerializeField] private int minNameLength = 3;
        [SerializeField] private int maxNameLength = 20;
        
        [Header("Game Mode")]
        [SerializeField] private Toggle peacefulModeToggle;
        
        private PlayerClass _selectedClass;
        private readonly List<GameObject> _classButtons = new List<GameObject>();
        private readonly List<PlayerClass> _availableClasses = new List<PlayerClass>();

        private ICharacterService _characters;
        
        public Action OnCharacterCreated;
        public Action OnCancelled;
        public Action<string, PlayerClass, bool> CreateRequested;
        
        private void Awake()
        {
            SetupUI();
        }
        
        private void SetupUI()
        {
            if (createButton)
            {
                createButton.onClick.AddListener(OnCreateButtonClicked);
            }
            
            if (cancelButton)
            {
                cancelButton.onClick.AddListener(OnCancelButtonClicked);
            }
            
            if (characterNameInput)
            {
                characterNameInput.onValueChanged.AddListener(OnNameInputChanged);
            }

            if (errorMessageText)
            {
                errorMessageText.gameObject.SetActive(false);
            }
            
            if (createButton)
            {
                createButton.interactable = false;
            }
        }
        
        public void SetAvailableClasses(List<PlayerClass> availableClasses)
        {
            
            if (!classButtonContainer || !classButtonPrefab)
            {
                return;
            }
            
            foreach (var button in _classButtons)
            {
                Destroy(button);
            }
            _classButtons.Clear();

            _availableClasses.Clear();
            if (availableClasses != null) _availableClasses.AddRange(availableClasses);

            if (_availableClasses.Count == 0)
            {
                return;
            }
            
            foreach (var playerClass in _availableClasses)
            {
                GameObject buttonObj = Instantiate(classButtonPrefab, classButtonContainer);
                Button button = buttonObj.GetComponent<Button>();
                
                if (button)
                {
                    Image buttonIcon = buttonObj.GetComponent<Image>();
                    if (buttonIcon && playerClass.ClassIcon)
                    {
                        buttonIcon.sprite = playerClass.ClassIcon;
                    }
                    
                    PlayerClass classRef = playerClass;
                    button.onClick.AddListener(() => OnClassSelected(classRef));
                    
                    _classButtons.Add(buttonObj);
                }
            }

            if (_availableClasses.Count > 0)
            {
                OnClassSelected(_availableClasses[0]);
            }
        }
        
        private void OnClassSelected(PlayerClass playerClass)
        {
            _selectedClass = playerClass;

            if (classNameText)
            {
                classNameText.text = playerClass.ClassName;
            }
            
            if (classDescriptionText)
            {
                classDescriptionText.text = playerClass.Description;
            }
            
            if (classIconImage && playerClass.ClassIcon)
            {
                classIconImage.sprite = playerClass.ClassIcon;
                classIconImage.gameObject.SetActive(true);
            }

            UpdateClassButtonStates();

            ValidateForm();
        }
        
        private void UpdateClassButtonStates()
        {
            foreach (var buttonObj in _classButtons)
            {
                Button button = buttonObj.GetComponent<Button>();
                if (button)
                {
                    ColorBlock colors = button.colors;
                    TextMeshProUGUI buttonText = buttonObj.GetComponentInChildren<TextMeshProUGUI>();
                    
                    if (buttonText && _selectedClass && 
                        buttonText.text == _selectedClass.ClassName)
                    {
                        colors.normalColor = new Color(0.8f, 0.8f, 1f);
                    }
                    else
                    {
                        colors.normalColor = Color.white;
                    }
                    
                    button.colors = colors;
                }
            }
        }
        
        private void OnNameInputChanged(string value)
        {
            ValidateForm();
        }
        
        private void ValidateForm()
        {
            bool isValid = true;
            string errorMessage = "";
            
            string characterName = characterNameInput ? characterNameInput.text.Trim() : "";
            
            if (string.IsNullOrWhiteSpace(characterName))
            {
                isValid = false;
                errorMessage = "Введите имя персонажа";
            }
            else if (characterName.Length < minNameLength)
            {
                isValid = false;
                errorMessage = $"Имя должно содержать минимум {minNameLength} символа";
            }
            else if (characterName.Length > maxNameLength)
            {
                isValid = false;
                errorMessage = $"Имя не должно превышать {maxNameLength} символов";
            }
            else if (_characters != null && _characters.IsCharacterNameTaken(characterName))
            {
                isValid = false;
                errorMessage = "Это имя уже занято";
            }
            
            if (!_selectedClass)
            {
                isValid = false;
                errorMessage = "Выберите класс";
            }
            
            if (createButton)
            {
                createButton.interactable = isValid;
            }
            
            if (errorMessageText)
            {
                if (!isValid && !string.IsNullOrEmpty(errorMessage))
                {
                    errorMessageText.text = errorMessage;
                    errorMessageText.gameObject.SetActive(true);
                }
                else
                {
                    errorMessageText.gameObject.SetActive(false);
                }
            }
        }
        
        private void OnCreateButtonClicked()
        {
            string characterName = characterNameInput.text.Trim();
            
            if (!_selectedClass)
            {
                ShowError("Выберите класс");
                return;
            }

            bool peacefulModeEnabled = peacefulModeToggle && peacefulModeToggle.isOn;
            CreateRequested?.Invoke(characterName, _selectedClass, peacefulModeEnabled);
        }
        
        private void OnCancelButtonClicked()
        {
            OnCancelled?.Invoke();
        }
        
        private void ShowError(string message)
        {
            if (errorMessageText)
            {
                errorMessageText.text = message;
                errorMessageText.gameObject.SetActive(true);
            }
        }
        
        public void ResetForm()
        {
            if (characterNameInput)
            {
                characterNameInput.text = "";
            }
            
            if (errorMessageText)
            {
                errorMessageText.gameObject.SetActive(false);
            }

            if (peacefulModeToggle)
            {
                peacefulModeToggle.isOn = false;
            }

            if (_availableClasses.Count > 0)
            {
                SetAvailableClasses(new List<PlayerClass>(_availableClasses));
            }
        }

        public void Initialize(ICharacterService characterService)
        {
            _characters = characterService;
        }

        public void ShowCreateFailed(string message = "Не удалось создать персонажа")
        {
            ShowError(message);
        }

    }
}
