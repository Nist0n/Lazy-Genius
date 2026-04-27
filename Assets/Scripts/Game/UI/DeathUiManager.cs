using System;
using SaveSystem;
using Player;
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

            var playerLoader = FindAnyObjectByType<PlayerCharacterLoader>();
            if (playerLoader)
            {
                playerLoader.SkipNextSaveOnDestroy();
            }

            if (CharacterManager.Instance)
            {
                CharacterData activeCharacter = CharacterManager.Instance.ActiveCharacter;
                if (activeCharacter != null)
                {
                    activeCharacter.HasGameplayState = false;
                    activeCharacter.Enemies?.Clear();
                    activeCharacter.PlayerPosition = Vector3.zero;

                    if (activeCharacter.MaxHealth <= 0f) activeCharacter.MaxHealth = 100f;
                    if (activeCharacter.MaxEnergy <= 0f) activeCharacter.MaxEnergy = 100f;

                    activeCharacter.CurrentHealth = activeCharacter.MaxHealth;
                    activeCharacter.CurrentEnergy = activeCharacter.MaxEnergy;
                }
            }

            SceneManager.LoadScene(gameplaySceneName);
        }
    }
}
