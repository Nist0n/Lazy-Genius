using System;
using Core;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class MainMenuButtonsController : MonoBehaviour
    {
        [SerializeField] private Button startGameButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button quitGameButton;
        [SerializeField] private GameObject menuButtons;
        [SerializeField] private GameObject settingsMenu;
        
        private GameFlowManager _gameFlowManager;

        private void Awake()
        {
            _gameFlowManager = GameFlowManager.Instance;
        }

        private void Start()
        {
            SetButtonsTriggers();
            
            settingsMenu.SetActive(false);
        }

        private void OpenSettings()
        {
            settingsMenu.SetActive(true);
            menuButtons.SetActive(false);
        }

        private void StartGamePreparation()
        {
            _gameFlowManager.StartGamePreparation();
            menuButtons.SetActive(false);
        }

        private void SetButtonsTriggers()
        {
            startGameButton.onClick.AddListener(StartGamePreparation);
            quitGameButton.onClick.AddListener(_gameFlowManager.QuitGame);
            settingsButton.onClick.AddListener(OpenSettings);
        }
    }
}
