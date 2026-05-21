using Abstractions.Audio;
using Abstractions.Characters;
using Composition;
using Core;
using Enemy;
using Game;
using Game.Events;
using Game.Input;
using Game.Score;
using Game.UI;
using Player;
using Player.UI;
using UI;
using UI.Enemy;
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
        [SerializeField] private EnemyRuntimeSpawner enemyRuntimeSpawner;
        [SerializeField] private KillScoreSystem killScoreSystem;
        [SerializeField] private BossSpawnOnScore bossSpawnOnScore;
        [SerializeField] private EnemyUIManager enemyUiManager;

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
            var eventBus = App.Services.Resolve<EventBus>();

            var abilitySlotSystem = playerController.GetComponent<AbilitySlotSystem>();
            var playerHealth = playerController.GetComponent<HealthSystem>();
            var playerInputHandler = playerController.GetComponent<PlayerInputHandler>();
            
            gameFlowManager.Initialize(audio, characters);

            playerHealth?.BindEventBus(eventBus);

            if (hudManager && playerHealth)
            {
                hudManager.Initialize(playerHealth, Camera.main);
            }

            if (killScoreSystem)
            {
                killScoreSystem.Initialize(audio, eventBus);

                if (hudManager)
                {
                    hudManager.OnKillScoreChanged(killScoreSystem.CurrentScore);
                    killScoreSystem.ScoreChanged += hudManager.OnKillScoreChanged;
                }

                if (bossSpawnOnScore)
                {
                    killScoreSystem.ScoreChanged += bossSpawnOnScore.OnScoreChanged;
                }
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

            enemyUiManager?.Initialize(eventBus);

            enemyRuntimeSpawner?.Initialize(eventBus);
            bossSpawnOnScore?.Initialize(eventBus);
            enemyRuntimeSpawner?.TrySpawnFromActiveCharacter();
            BindEnemyHealthInScene(eventBus);

            if (settingsController && _settingsTabsController == null)
            {
                _settingsTabsController = new SettingsTabsController(settingsController);
            }
            
            if (characterSelectionUI && _characterSelectionController == null)
            {
                _characterSelectionController = new CharacterSelectionController(characterSelectionUI, characters);
            }
        }

        private static void BindEnemyHealthInScene(EventBus eventBus)
        {
            var enemies = FindObjectsByType<EnemyHealth>(FindObjectsSortMode.None);
            for (var i = 0; i < enemies.Length; i++)
            {
                enemies[i].BindEventBus(eventBus);
            }
        }

        private void OnDestroy()
        {
            if (killScoreSystem)
            {
                if (hudManager)
                {
                    killScoreSystem.ScoreChanged -= hudManager.OnKillScoreChanged;
                }

                if (bossSpawnOnScore)
                {
                    killScoreSystem.ScoreChanged -= bossSpawnOnScore.OnScoreChanged;
                }
            }

            _settingsTabsController?.Dispose();
            _settingsTabsController = null;
            
            _characterSelectionController?.Dispose();
            _characterSelectionController = null;
        }
    }
}

