using UnityEngine;
using UnityEngine.UI;
using Game;
using SaveSystem;
using UnityEngine.SceneManagement;

namespace Game.UI
{
    public class PauseMenuUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button saveButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button mainMenuButton;
        [SerializeField] private Button quitButton;
        
        [Header("Panels")]
        [SerializeField] private GameObject settingsPanel;
        
        private PauseManager _pauseManager;
        
        private void Start()
        {
            _pauseManager = PauseManager.Instance;
            
            if (!_pauseManager)
            {
                Debug.LogError("[PauseMenuUI] PauseManager не найден в сцене!");
                return;
            }
            
            if (_pauseManager)
            {
                var pauseManagerType = typeof(PauseManager);
                var pauseMenuPanelField = pauseManagerType.GetField("pauseMenuPanel", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                
                if (pauseMenuPanelField != null)
                {
                    pauseMenuPanelField.SetValue(_pauseManager, gameObject);
                }
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
            
            gameObject.SetActive(false);
            if (settingsPanel) settingsPanel.SetActive(false);
        }
        
        private void OnDestroy()
        {
            if (resumeButton) resumeButton.onClick.RemoveListener(OnResumeClicked);
            if (saveButton) saveButton.onClick.RemoveListener(OnSaveClicked);
            if (settingsButton) settingsButton.onClick.RemoveListener(OnSettingsClicked);
            if (mainMenuButton) mainMenuButton.onClick.RemoveListener(OnMainMenuClicked);
            if (quitButton) quitButton.onClick.RemoveListener(OnQuitClicked);
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
            if (CharacterManager.Instance)
            {
                if (CharacterManager.Instance.SaveActiveCharacter())
                {
                    Debug.Log("[PauseMenuUI] Игра сохранена!");
                    // TODO: Показать визуальное подтверждение
                }
            }
        }
        
        public bool IsSettingsOpen => settingsPanel && settingsPanel.activeSelf;

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
        
        private void OnMainMenuClicked()
        {
            if (_pauseManager)
            {
                _pauseManager.ResumeGame();
            }
            
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            
            if (CharacterManager.Instance)
            {
                CharacterManager.Instance.DeselectCharacter();
            }
            
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
    }
}

