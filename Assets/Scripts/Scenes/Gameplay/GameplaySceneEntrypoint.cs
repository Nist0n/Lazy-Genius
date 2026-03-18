using Abstractions.Characters;
using Composition;
using Game;
using Game.Input;
using Game.UI;
using Player;
using Player.UI;
using UI.HUD;
using UI.Settings;
using UI.Settings.MVC;
using UnityEngine;

namespace Scenes.Gameplay
{
    [DefaultExecutionOrder(-9999)]
    public sealed class GameplaySceneEntrypoint : MonoBehaviour
    {
        [Header("Scene Objects")]
        [SerializeField] private PlayerController playerController;
        [SerializeField] private HUDManager hudManager;
        [SerializeField] private AbilitySlotsUIManager abilitySlotsUiManager;
        [SerializeField] private KeyBindingSettingsManager keyBindingSettingsManager;
        [SerializeField] private InputOverrideLoader inputOverrideLoader;
        [SerializeField] private PauseManager pauseManager;
        [SerializeField] private PauseMenuUI pauseMenuUi;
        [SerializeField] private SettingsController settingsController;

        private SettingsTabsController _settingsTabsController;

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
            if (!App.IsInitialized)
            {
                return;
            }

            if (!playerController)
            {
                return;
            }

            var abilitySlotSystem = playerController.GetComponent<AbilitySlotSystem>();
            var playerHealth = playerController.GetComponent<HealthSystem>();
            var playerInputHandler = playerController.GetComponent<PlayerInputHandler>();

            if (hudManager && playerHealth)
            {
                hudManager.Initialize(playerHealth, Camera.main);
            }

            if (abilitySlotsUiManager && abilitySlotSystem)
            {
                abilitySlotsUiManager.Initialize(abilitySlotSystem);
            }

            if (keyBindingSettingsManager)
            {
                keyBindingSettingsManager.Initialize(abilitySlotSystem, playerInputHandler);
            }

            if (inputOverrideLoader && playerInputHandler)
            {
                inputOverrideLoader.Initialize(playerInputHandler.GetInputActionAsset());
            }

            if (pauseMenuUi && pauseManager)
            {
                var characters = App.Services.Resolve<ICharacterService>();
                pauseMenuUi.Initialize(pauseManager, characters);
            }

            if (settingsController && _settingsTabsController == null)
            {
                _settingsTabsController = new SettingsTabsController(settingsController);
            }
        }

        private void OnDestroy()
        {
            _settingsTabsController?.Dispose();
            _settingsTabsController = null;
        }
    }
}

