using System;
using Abstractions.Audio;
using Abstractions.Characters;
using UI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Core
{
    public class GameFlowManager : MonoBehaviour
    {
        [Header("Scene Names")]
        [SerializeField] private string gameplaySceneName = "GameplayScene";
        
        [Header("UI References")]
        [SerializeField] private GameObject characterSelectionPanel;
        [SerializeField] private GameObject characterCreationPanel;

        [Header("Scripts")]
        [SerializeField] private CharacterSelectionUI characterSelectionUI;
        [SerializeField] private CharacterCreationUI characterCreationUI;

        private IAudioService _audio;
        private ICharacterService _characters;
        
        private void Start()
        {
            if (_audio == null || _characters == null)
            {
                return;
            }

            _audio.PlayMusic("MenuMusic");
        }

        private void OnEnable()
        {
            if (characterSelectionUI)
            {
                characterSelectionUI.OnCreateNewCharacter += ShowCharacterCreation;
                characterSelectionUI.OnCharacterSelected += StartGame;
            }

            if (characterCreationUI)
            {
                characterCreationUI.OnCharacterCreated += ShowCharacterSelection;
                characterCreationUI.OnCancelled += ShowCharacterSelection;
            }
        }

        private void OnDisable()
        {
            if (characterSelectionUI)
            {
                characterSelectionUI.OnCreateNewCharacter -= ShowCharacterCreation;
                characterSelectionUI.OnCharacterSelected -= StartGame;
            }
            
            if (characterCreationUI)
            {
                characterCreationUI.OnCharacterCreated -= ShowCharacterSelection;
                characterCreationUI.OnCancelled -= ShowCharacterSelection;
            }
        }

        public void StartGamePreparation()
        {
            if (_characters == null)
            {
                return;
            }

            var characters = _characters.GetCharacterList();
            
            if (characters.Count == 0)
            {
                ShowCharacterCreation();
            }
            else
            {
                ShowCharacterSelection();
            }
        }
        
        public void ShowCharacterSelection()
        {
            if (characterSelectionPanel)
            {
                characterSelectionPanel.SetActive(true);
            }
            
            if (characterCreationPanel)
            {
                characterCreationPanel.SetActive(false);
            }
        }

        public void ShowCharacterCreation()
        {
            if (characterSelectionPanel)
            {
                characterSelectionPanel.SetActive(false);
            }
            
            if (characterCreationPanel)
            {
                characterCreationPanel.SetActive(true);
            }
        }

        private void StartGame(string characterGuid)
        {
            if (_characters == null)
            {
                return;
            }

            if (!_characters.SelectCharacter(characterGuid))
            {
                return;
            }
            
            _audio.PlayMusic("StartGameClick");
            
            LoadGameplayScene();
        }
        
        private void LoadGameplayScene()
        {
            SceneManager.LoadScene(gameplaySceneName);
        }
        
        public void QuitGame()
        {
            if (_characters == null)
            {
                return;
            }

            _characters.SaveActiveCharacter();
            
            #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
            #else
            Application.Quit();
            #endif
        }

        public void Initialize(IAudioService audioService, ICharacterService characterService)
        {
            _audio = audioService;
            _characters = characterService;
        }
    }
}
