using Abstractions.Audio;
using Abstractions.Characters;
using Abstractions.Save;
using Composition;
using Core;
using Infrastructure.Services;
using SaveSystem;
using UI;
using UI.Settings;
using UI.Settings.MVC;
using UI.MainMenu.MVC;
using UnityEngine;

namespace Scenes.MainMenu
{
    public sealed class MainMenuSceneEntrypoint : MonoBehaviour
    {
        [Header("Scene Objects")]
        [SerializeField] private GameFlowManager gameFlowManager;
        [SerializeField] private MainMenuButtonsController mainMenuButtonsController;
        [SerializeField] private SettingsController settingsController;
        [SerializeField] private CharacterSelectionUI characterSelectionUI;
        [SerializeField] private CharacterCreationUI characterCreationUI;

        private SettingsTabsController _settingsTabsController;
        private CharacterSelectionController _characterSelectionController;
        private CharacterCreationController _characterCreationController;

        private void Awake()
        {
            TryWire();
        }

        private void Start()
        {
            TryWire();
        }

        private void TryWire()
        {
            if (!App.IsInitialized) return;

            if (!gameFlowManager)
            {
                return;
            }

            var audio = App.Services.Resolve<IAudioService>();
            var characters = App.Services.Resolve<ICharacterService>();
            var saves = App.Services.Resolve<ICharacterSaveService>();

            gameFlowManager.Initialize(audio, characters);

            if (CharacterManager.Instance)
            {
                CharacterManager.Instance.Initialize(saves);
            }

            if (mainMenuButtonsController)
            {
                mainMenuButtonsController.Initialize(gameFlowManager);
            }

            if (settingsController && _settingsTabsController == null)
            {
                _settingsTabsController = new SettingsTabsController(settingsController);
            }

            if (characterSelectionUI && _characterSelectionController == null)
            {
                _characterSelectionController = new CharacterSelectionController(characterSelectionUI, characters);
            }

            if (characterCreationUI && _characterCreationController == null)
            {
                Debug.Log(characters);
                _characterCreationController = new CharacterCreationController(characterCreationUI, characters);
            }
        }

        private void OnDestroy()
        {
            _settingsTabsController?.Dispose();
            _settingsTabsController = null;

            _characterSelectionController?.Dispose();
            _characterSelectionController = null;

            _characterCreationController?.Dispose();
            _characterCreationController = null;
        }
    }
}

