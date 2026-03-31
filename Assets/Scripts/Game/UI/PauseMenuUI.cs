using UnityEngine;
using UnityEngine.UI;
using Game;
using UnityEngine.SceneManagement;
using Abstractions.Characters;
using Player;

namespace Game.UI
{
    public class PauseMenuUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button saveButton;
        [SerializeField] private Button loadButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button mainMenuButton;
        [SerializeField] private Button quitButton;
        
        [Header("Panels")]
        [SerializeField] private GameObject settingsPanel;
        [SerializeField] private GameObject selectionPanel;
        
        private PauseManager _pauseManager;
        private ICharacterService _characters;
        
        private void Start()
        {
            if (!_pauseManager || _characters == null)
            {
                return;
            }

            if (resumeButton)
            {
                resumeButton.onClick.AddListener(OnResumeClicked);
            }
            
            if (saveButton)
            {
                saveButton.onClick.AddListener(OnSaveClicked);
            }
            
            if (settingsButton)
            {
                settingsButton.onClick.AddListener(OnSettingsClicked);
            }
            
            if (mainMenuButton)
            {
                mainMenuButton.onClick.AddListener(OnMainMenuClicked);
            }
            
            if (quitButton)
            {
                quitButton.onClick.AddListener(OnQuitClicked);
            }
            
            if (loadButton)
            {
                loadButton.onClick.AddListener(OnLoadClicked);
            }
            
            gameObject.SetActive(false);
            if (settingsPanel) settingsPanel.SetActive(false);
        }

        public void Initialize(PauseManager pauseManager, ICharacterService characterService)
        {
            _pauseManager = pauseManager;
            _characters = characterService;
        }
        
        private void OnDestroy()
        {
            if (resumeButton) resumeButton.onClick.RemoveListener(OnResumeClicked);
            if (saveButton) saveButton.onClick.RemoveListener(OnSaveClicked);
            if (settingsButton) settingsButton.onClick.RemoveListener(OnSettingsClicked);
            if (mainMenuButton) mainMenuButton.onClick.RemoveListener(OnMainMenuClicked);
            if (quitButton) quitButton.onClick.RemoveListener(OnQuitClicked);
            if (loadButton) loadButton.onClick.RemoveListener(OnLoadClicked);
        }
        
        private void OnResumeClicked()
        {
            if (_pauseManager)
            {
                _pauseManager.ResumeGame();
            }
        }
        
        private void OnSaveClicked()
        {
            var loader = FindAnyObjectByType<PlayerCharacterLoader>();
            if (loader)
            {
                loader.SaveCharacterData();
                Debug.Log("Игра сохранена");
                return;
            }

            if (_characters.SaveActiveCharacter())
            {
                Debug.Log("Игра сохранена");
            }
        }
        
        public bool IsSettingsOpen => settingsPanel && settingsPanel.activeSelf;
        public bool IsSelectionOpen => selectionPanel && selectionPanel.activeSelf;

        private void OnSettingsClicked()
        {
            if (settingsPanel)
            {
                settingsPanel.SetActive(true);
                gameObject.SetActive(false);
            }
        }
        
        public void ReturnFromSettings()
        {
            if (settingsPanel) settingsPanel.SetActive(false);
            gameObject.SetActive(true);
        }
        
        public void ReturnFromSelection()
        {
            if (selectionPanel) selectionPanel.SetActive(false);
            gameObject.SetActive(true);
        }
        
        private void OnMainMenuClicked()
        {
            if (_pauseManager)
            {
                _pauseManager.ResumeGame();
            }
            
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            
            _characters.DeselectCharacter();
            
            SceneManager.LoadScene("MainMenuScene");
        }
        
        private void OnQuitClicked()
        {
            if (_pauseManager)
            {
                _pauseManager.ResumeGame();
            }
            
            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
            #else
                Application.Quit();
            #endif
        }

        private void OnLoadClicked()
        {
            if (selectionPanel)
            {
                selectionPanel.SetActive(true);
                gameObject.SetActive(false);
            }
        }
    }
}

