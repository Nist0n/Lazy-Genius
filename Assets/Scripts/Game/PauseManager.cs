using Game.UI;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game
{
    public class PauseManager : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private KeyCode pauseKey = KeyCode.Escape;
        [SerializeField] private bool useInputSystem = true;
        
        [Header("UI")]
        [SerializeField] private GameObject pauseMenuPanel;
        
        private bool _isPaused = false;
        private CursorLockMode _previousLockState;
        private bool _previousCursorVisible;
        
        public bool IsPaused => _isPaused;
        
        public static PauseManager Instance { get; private set; }
        
        private void Awake()
        {
            if (Instance && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            Instance = this;
        }
        
        private PauseMenuUI _pauseMenuUI;
        
        private void Start()
        {
            _previousLockState = Cursor.lockState;
            _previousCursorVisible = Cursor.visible;
            
            if (pauseMenuPanel)
            {
                _pauseMenuUI = pauseMenuPanel.GetComponent<PauseMenuUI>();
                pauseMenuPanel.SetActive(false);
            }
        }
        
        private void Update()
        {
            if (useInputSystem)
            {
                if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
                {
                    HandlePauseInput();
                }
            }
        }
        
        private void HandlePauseInput()
        {
            if (_isPaused && _pauseMenuUI && _pauseMenuUI.IsSettingsOpen)
            {
                _pauseMenuUI.ReturnFromSettings();
            }
            else
            {
                TogglePause();
            }
        }
        
        public void TogglePause()
        {
            if (_isPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame(true);
            }
        }
        
        public void PauseGame(bool withPanel)
        {
            if (_isPaused) return;
            
            if (pauseMenuPanel && withPanel)
            {
                pauseMenuPanel.SetActive(true);
            }
            
            _isPaused = true;
            
            _previousLockState = Cursor.lockState;
            _previousCursorVisible = Cursor.visible;
            
            Time.timeScale = 0f;
            
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        
        public void ResumeGame()
        {
            if (!_isPaused) return;
            
            _isPaused = false;
            
            Time.timeScale = 1f;
            
            Cursor.lockState = _previousLockState;
            Cursor.visible = _previousCursorVisible;
            
            if (_pauseMenuUI && _pauseMenuUI.IsSettingsOpen)
            {
                _pauseMenuUI.ReturnFromSettings();
            }
            
            if (pauseMenuPanel)
            {
                pauseMenuPanel.SetActive(false);
            }
        }
        
        public void SetPauseKey(KeyCode newKey)
        {
            pauseKey = newKey;
        }
        
        private void OnDestroy()
        {
            if (_isPaused)
            {
                Time.timeScale = 1f;
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            
            if (Instance == this)
            {
                Instance = null;
            }
        }
        
        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus && !_isPaused)
            {
                PauseGame(true);
            }
        }
    }
}

