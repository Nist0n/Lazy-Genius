using System;
using Audio;
using SaveSystem;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class CharacterCardUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI characterNameText;
        [SerializeField] private TextMeshProUGUI classNameText;
        [SerializeField] private TextMeshProUGUI levelText;
        [SerializeField] private TextMeshProUGUI lastPlayedText;
        [SerializeField] private TextMeshProUGUI playtimeText;
        [SerializeField] private Image classIconImage;
        [SerializeField] private Button selectButton;
        [SerializeField] private Button deleteButton;
        
        [Header("Visual Settings")]
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Color hoverColor = new Color(1f, 1f, 1f, 0.1f);
        
        private CharacterMetadata _metadata;
        private Color _originalBackgroundColor;
        
        public Action OnSelectClicked;
        public Action OnDeleteClicked;
        
        private void Awake()
        {
            if (backgroundImage)
            {
                _originalBackgroundColor = backgroundImage.color;
            }
            
            if (selectButton)
            {
                selectButton.onClick.AddListener(HandleSelectClicked);
            }
            
            if (deleteButton)
            {
                deleteButton.onClick.AddListener(HandleDeleteClicked);
            }
        }

        public void Initialize(CharacterMetadata characterMetadata)
        {
            _metadata = characterMetadata;
            UpdateDisplay();
        }
        
        private void UpdateDisplay()
        {
            if (_metadata == null) return;

            if (characterNameText)
            {
                characterNameText.text = _metadata.characterName;
            }

            if (classNameText)
            {
                classNameText.text = _metadata.className;
            }

            if (levelText)
            {
                levelText.text = $"Уровень {_metadata.currentLevel}";
            }
            
            if (lastPlayedText)
            {
                lastPlayedText.text = $"Последняя игра: {_metadata.GetLastPlayedDate()}";
            }
            
            if (playtimeText)
            {
                playtimeText.text = $"Время игры: {_metadata.GetFormattedPlaytime()}";
            }
            
            if (classIconImage && _metadata.classIcon)
            {
                classIconImage.sprite = _metadata.classIcon;
                classIconImage.gameObject.SetActive(true);
            }
            else if (classIconImage)
            {
                classIconImage.gameObject.SetActive(false);
            }
        }
        
        private void HandleSelectClicked()
        {
            AudioManager.Instance.PlaySFX("StartGameClick");
            OnSelectClicked?.Invoke();
        }
        
        private void HandleDeleteClicked()
        {
            OnDeleteClicked?.Invoke();
        }
        
        public void OnPointerEnter()
        {
            if (backgroundImage)
            {
                backgroundImage.color = _originalBackgroundColor + hoverColor;
            }
        }
        
        public void OnPointerExit()
        {
            if (backgroundImage)
            {
                backgroundImage.color = _originalBackgroundColor;
            }
        }
    }
}
