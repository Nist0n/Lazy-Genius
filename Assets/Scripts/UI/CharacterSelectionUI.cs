using System;
using System.Collections.Generic;
using Abstractions.Characters;
using SaveSystem;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace UI
{
    public class CharacterSelectionUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Transform characterListContainer;
        [SerializeField] private GameObject characterCardPrefab;
        [SerializeField] private Button createNewCharacterButton;
        [SerializeField] private GameObject emptyStatePanel;
        [SerializeField] private TextMeshProUGUI emptyStateText;
        
        [Header("Confirmation Dialog")]
        [SerializeField] private GameObject deleteConfirmationDialog;
        [SerializeField] private TextMeshProUGUI deleteConfirmationText;
        [SerializeField] private Button confirmDeleteButton;
        [SerializeField] private Button cancelDeleteButton;
        
        private List<GameObject> _characterCards = new List<GameObject>();
        private string _characterToDelete;
        
        public Action<string> OnCharacterSelected;
        public Action OnCreateNewCharacter;
        public Action<string> OnDeleteConfirmed;

        private ICharacterService _characters;
        
        private void Awake()
        {
            SetupUI();
        }
        
        private void OnDestroy()
        {
            _characters = null;
        }
        
        private void SetupUI()
        {
            if (createNewCharacterButton)
            {
                createNewCharacterButton.onClick.AddListener(OnCreateNewCharacterClicked);
            }
            
            if (deleteConfirmationDialog)
            {
                deleteConfirmationDialog.SetActive(false);
            }
            
            if (confirmDeleteButton)
            {
                confirmDeleteButton.onClick.AddListener(OnConfirmDeleteClicked);
            }
            
            if (cancelDeleteButton)
            {
                cancelDeleteButton.onClick.AddListener(OnCancelDeleteClicked);
            }
        }
        
        public void RefreshCharacterList()
        {
            foreach (var card in _characterCards)
            {
                Destroy(card);
            }
            _characterCards.Clear();

            if (_characters == null)
            {
                ShowEmptyState(true);
                return;
            }

            List<CharacterMetadata> characters = _characters.GetCharacterList();
            
            if (characters.Count == 0)
            {
                ShowEmptyState(true);
                return;
            }
            
            ShowEmptyState(false);

            foreach (var character in characters)
            {
                CreateCharacterCard(character);
            }
        }
        
        private void CreateCharacterCard(CharacterMetadata metadata)
        {
            if (!characterCardPrefab || !characterListContainer)
            {
                return;
            }
            
            GameObject cardObj = Instantiate(characterCardPrefab, characterListContainer);
            CharacterCardUI cardUI = cardObj.GetComponent<CharacterCardUI>();
            
            if (cardUI)
            {
                cardUI.Initialize(metadata);
                cardUI.OnSelectClicked += () => OnCharacterCardSelected(metadata.characterGuid);
                cardUI.OnDeleteClicked += () => OnCharacterCardDeleteClicked(metadata.characterGuid, metadata.characterName);
            }
            
            _characterCards.Add(cardObj);
        }
        
        private void ShowEmptyState(bool show)
        {
            if (emptyStatePanel)
            {
                emptyStatePanel.SetActive(show);
            }
            
            if (characterListContainer)
            {
                characterListContainer.gameObject.SetActive(!show);
            }
        }
        
        private void OnCharacterCardSelected(string characterGuid)
        {
            OnCharacterSelected?.Invoke(characterGuid);
        }
        
        private void OnCharacterCardDeleteClicked(string characterGuid, string characterName)
        {
            if (SceneManager.GetActiveScene().name != "MainMenuScene") return;
                
            _characterToDelete = characterGuid;
            
            if (deleteConfirmationDialog)
            {
                deleteConfirmationDialog.SetActive(true);
            }
            
            if (deleteConfirmationText)
            {
                deleteConfirmationText.text = $"Вы уверены, что хотите удалить персонажа '{characterName}'?\nЭто действие нельзя отменить.";
            }
        }
        
        private void OnConfirmDeleteClicked()
        {
            if (!string.IsNullOrEmpty(_characterToDelete))
            {
                OnDeleteConfirmed?.Invoke(_characterToDelete);
                _characterToDelete = null;
            }
            
            if (deleteConfirmationDialog)
            {
                deleteConfirmationDialog.SetActive(false);
            }
        }
        
        private void OnCancelDeleteClicked()
        {
            _characterToDelete = null;
            
            if (deleteConfirmationDialog)
            {
                deleteConfirmationDialog.SetActive(false);
            }
        }
        
        private void OnCreateNewCharacterClicked()
        {
            OnCreateNewCharacter?.Invoke();
        }

        public void Initialize(ICharacterService characterService)
        {
            _characters = characterService;
        }
    }
}
