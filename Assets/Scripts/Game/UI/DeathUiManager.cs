using System;
using SaveSystem;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Game.UI
{
    public class DeathUiManager : MonoBehaviour
    {
        [SerializeField] private Button menuButton;
        [SerializeField] private Button lobbyButton;
        [SerializeField] private GameObject deathPanel;
        [SerializeField] private string gameplaySceneName = "GameplayScene";
        
        private void Start()
        {
            deathPanel.SetActive(false);
            menuButton.onClick.AddListener(OnMenuButtonClicked);
            lobbyButton.onClick.AddListener(OnLobbyButtonClicked);
        }

        private void OnMenuButtonClicked()
        {
            if (CharacterManager.Instance)
            {
                CharacterManager.Instance.SaveActiveCharacter();
                CharacterManager.Instance.DeselectCharacter();
            }
            
            PauseManager.Instance.ResumeGame();
            
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            
            SceneManager.LoadScene("MainMenuScene");
        }
        
        private void OnLobbyButtonClicked()
        {
            PauseManager.Instance.ResumeGame();
            
            if (CharacterManager.Instance)
            {
                CharacterManager.Instance.SaveActiveCharacter();
            }
            
            SceneManager.LoadScene(gameplaySceneName);
        }
    }
}
