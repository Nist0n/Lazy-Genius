using System;
using Audio;
using SaveSystem;
using UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

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
        
        private static GameFlowManager _instance;
        
        public static GameFlowManager Instance
        {
            get
            {
                if (!_instance)
                {
                    _instance = FindAnyObjectByType<GameFlowManager>();
                }
                return _instance;
            }
        }
        
        private void Awake()
        {
            if (_instance && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            _instance = this;
        }

        private void Start()
        {
            AudioManager.Instance.PlayMusic("MenuMusic");
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
            if (!SaveManager.Instance)
            {
                Debug.LogError("[GameFlowManager] SaveManager отсутствует");
                return;
            }
            
            if (!CharacterManager.Instance)
            {
                Debug.LogError("[GameFlowManager] CharacterManager отсутствует");
                return;
            }
            
            var characters = CharacterManager.Instance.GetCharacterList();
            
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
        
        public void StartGame(string characterGuid)
        {
            if (!CharacterManager.Instance.SelectCharacter(characterGuid))
            {
                Debug.LogError($"[GameFlowManager] Не удалось выбрать персонажа: {characterGuid}");
                return;
            }
            
            Debug.Log($"[GameFlowManager] Начало игры с персонажем: {characterGuid}");
            
            LoadGameplayScene();
        }
        
        private void LoadGameplayScene()
        {
            SceneManager.LoadScene(gameplaySceneName);
        }
        
        public void QuitGame()
        {
            CharacterManager.Instance.SaveActiveCharacter();
            
            #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
            #else
            Application.Quit();
            #endif
        }
    }
}
