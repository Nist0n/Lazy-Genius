using Abstractions.Audio;
using Abstractions.Characters;
using Composition;
using Core;
using Game;
using Game.Input;
using Game.UI;
using Player;
using Player.UI;
using SaveSystem;
using UI;
using UI.HUD;
using UI.MainMenu.MVC;
using UI.Settings;
using UI.Settings.MVC;
using UnityEngine;

namespace Scenes.Gameplay
{
    [DefaultExecutionOrder(-9999)]
    public sealed class GameplaySceneEntrypoint : MonoBehaviour
    {
        [Header("Scene Objects")]
        [SerializeField] private GameFlowManager gameFlowManager;
        [SerializeField] private PlayerController playerController;
        [SerializeField] private HUDManager hudManager;
        [SerializeField] private AbilitySlotsUIManager abilitySlotsUiManager;
        [SerializeField] private KeyBindingSettingsManager keyBindingSettingsManager;
        [SerializeField] private InputOverrideLoader inputOverrideLoader;
        [SerializeField] private PauseManager pauseManager;
        [SerializeField] private PauseMenuUI pauseMenuUi;
        [SerializeField] private CharacterSelectionUI characterSelectionUI;
        [SerializeField] private SettingsController settingsController;

        private SettingsTabsController _settingsTabsController;
        private CharacterSelectionController _characterSelectionController;

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
            
            var characters = App.Services.Resolve<ICharacterService>();
            var audio = App.Services.Resolve<IAudioService>();

            var abilitySlotSystem = playerController.GetComponent<AbilitySlotSystem>();
            var playerHealth = playerController.GetComponent<HealthSystem>();
            var playerInputHandler = playerController.GetComponent<PlayerInputHandler>();
            
            gameFlowManager.Initialize(audio, characters);

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
                pauseMenuUi.Initialize(pauseManager, characters);
            }

            TryApplyEnemyPositionsFromSave();

            if (settingsController && _settingsTabsController == null)
            {
                _settingsTabsController = new SettingsTabsController(settingsController);
            }
            
            if (characterSelectionUI && _characterSelectionController == null)
            {
                _characterSelectionController = new CharacterSelectionController(characterSelectionUI, characters);
            }
        }

        private void TryApplyEnemyPositionsFromSave()
        {
            if (!CharacterManager.Instance) return;
            if (!CharacterManager.Instance.HasActiveCharacter) return;

            var character = CharacterManager.Instance.ActiveCharacter;
            if (character == null) return;
            if (!character.HasGameplayState) return;
            if (character.Enemies == null || character.Enemies.Count == 0) return;

            var applier = new EnemySaveApplier();
            applier.Apply(character.Enemies);
        }

        private void OnDestroy()
        {
            _settingsTabsController?.Dispose();
            _settingsTabsController = null;
            
            _characterSelectionController?.Dispose();
            _characterSelectionController = null;
        }
    }
}

