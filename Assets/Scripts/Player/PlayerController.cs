using Audio;
using Game;
using Player.Settings;
using Player.States;
using SaveSystem;
using UI.Settings;
using UnityEngine;

namespace Player
{
    [RequireComponent(typeof(PlayerMovement))]
    [RequireComponent(typeof(HealthSystem))]
    [RequireComponent(typeof(AbilitySystem))]
    [RequireComponent(typeof(AbilitySlotSystem))]
    [RequireComponent(typeof(PlayerInputHandler))]
    [RequireComponent(typeof(PlayerStateMachine))]
    public class PlayerController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PlayerStats baseStats;
        [SerializeField] private PlayerClass currentClass;
        
        [Header("Components")]
        private PlayerMovement _movement;
        private HealthSystem _healthSystem;
        private AbilitySystem _abilitySystem;
        private AbilitySlotSystem _abilitySlotSystem;
        private PlayerInputHandler _inputHandler;
        private PlayerStateMachine _stateMachine;
        
        [Header("Camera")]
        [SerializeField] private PlayerCameraController cameraController;
        [SerializeField] private Transform aimTarget;
        
        [Header("UI")] 
        [SerializeField] private GameObject deathCanvas;

        [Header("Sound")] 
        [SerializeField] private AudioSource audioSource;
        
        public PlayerStats BaseStats => baseStats;
        public AudioSource AudioOutput => audioSource;
        public PlayerClass CurrentClass => currentClass;
        public PlayerMovement Movement => _movement;
        public HealthSystem HealthSystem => _healthSystem;
        public AbilitySystem AbilitySystem => _abilitySystem;
        public AbilitySlotSystem AbilitySlotSystem => _abilitySlotSystem;
        public PlayerInputHandler InputHandler => _inputHandler;
        public PlayerStateMachine StateMachine => _stateMachine;

        private Camera _mainCamera;
        
        private void Awake()
        {
            InitializeComponents();
            _mainCamera = Camera.main;
        }
        
        private void Start()
        {
            InitializeSystems();
        }
        
        private void Update()
        {
            AimTargetFollow();
            HandleInput();
            currentClass?.OnUpdate(this);
        }
        
        private void InitializeComponents()
        {
            _movement = GetComponent<PlayerMovement>();
            _healthSystem = GetComponent<HealthSystem>();
            _abilitySystem = GetComponent<AbilitySystem>();
            _abilitySlotSystem = GetComponent<AbilitySlotSystem>();
            _inputHandler = GetComponent<PlayerInputHandler>();
            _stateMachine = GetComponent<PlayerStateMachine>();
            
            if (!cameraController)
            {
                cameraController = GetComponent<PlayerCameraController>();
                if (!cameraController)
                {
                    cameraController = gameObject.AddComponent<PlayerCameraController>();
                }
            }
        }

        private void InitializeSystems()
        {
            if (baseStats)
            {
                InitializeStats();
            }
            
            InitializeStateMachine();
            
            if (currentClass)
            {
                SetClass(currentClass);
            }

            _abilitySystem.Initialize(_healthSystem);
            
            SubscribeToEvents();
        }
        
        private void InitializeStateMachine()
        {
            _stateMachine.RegisterState(PlayerState.Idle, new IdleState(this));
            _stateMachine.RegisterState(PlayerState.Walking, new WalkingState(this));
            _stateMachine.RegisterState(PlayerState.Running, new RunningState(this));
            _stateMachine.RegisterState(PlayerState.Jumping, new JumpingState(this));
            _stateMachine.RegisterState(PlayerState.Falling, new FallingState(this));
            _stateMachine.RegisterState(PlayerState.Dead, new DeadState(this));

            _stateMachine.ChangeState(PlayerState.Idle);
        }
        
        private void InitializeStats()
        {
            float maxHealth = baseStats.maxHealth;
            _healthSystem.Initialize(maxHealth, baseStats.maxEnergy);
            
            _movement.SetMoveSpeed(baseStats.baseMoveSpeed);
            _movement.SetJumpForce(baseStats.jumpForce);
        }
        
        public void SetClass(PlayerClass newClass)
        {
            if (!newClass) return;
            
            currentClass = newClass;
            currentClass.Initialize(this);
        }
        
        private void HandleInput()
        {
            if (!_inputHandler) return;

            _movement.SetMoveInput(_inputHandler.MoveInput);
            _movement.SetJumpInput(_inputHandler.JumpPressed);
            _movement.SetSprintInput(_inputHandler.SprintPressed);

            if (_inputHandler.JumpPressed && _movement.IsGrounded && _stateMachine.CurrentStateType != PlayerState.Jumping)
            {
                _stateMachine.ChangeState(PlayerState.Jumping);
            }
            
            if (cameraController)
            {
                Transform orientation = cameraController.GetOrientation();
                if (orientation)
                {
                    _movement.SetOrientation(orientation);
                }
                
                Transform camTarget = cameraController.GetCameraFollowTarget();
                if (camTarget && _mainCamera)
                {
                    _movement.SetCameraTransform(_mainCamera.transform);
                }
            }
        }
        
        private void SubscribeToEvents()
        {
            if (_healthSystem)
            {
                _healthSystem.OnDeath += OnPlayerDeath;
            }
            
            if (_inputHandler && _abilitySlotSystem)
            {
                InitializeAbilitySlotBindings();
            }
            else if (!_inputHandler)
            {
                Debug.LogError("[PlayerController] PlayerInputHandler");
            }
            else if (!_abilitySlotSystem)
            {
                if (_inputHandler)
                {
                    _inputHandler.OnAbility1Pressed += () => _abilitySystem.UseAbility(0);
                    _inputHandler.OnAbility2Pressed += () => _abilitySystem.UseAbility(1);
                    _inputHandler.OnAbility3Pressed += () => _abilitySystem.UseAbility(2);
                }
            }
        }
        
        private void OnPlayerDeath()
        {
            AudioManager.Instance.PlaySFX("Defeat");
            PauseManager.Instance.PauseGame(false);
            deathCanvas.SetActive(true);
            _movement.SetDead(true);
            _stateMachine.ChangeState(PlayerState.Dead);
        }
        
        private void InitializeAbilitySlotBindings()
        {
            if (!_inputHandler || !_abilitySlotSystem) return;
            
            var settingsManager = FindAnyObjectByType<KeyBindingSettingsManager>();
            
            if (settingsManager)
            {
                settingsManager.ApplySettings();
            }
            else
            {
                ApplyDefaultBindings();
            }
        }
        
        private void ApplyDefaultBindings()
        {
            if (!_inputHandler || !_abilitySlotSystem) return;
            
            var inputActions = _inputHandler.GetInputActionAsset();
            if (!inputActions) return;
            
            var playerActionMap = _inputHandler.GetPlayerActionMap();
            if (playerActionMap == null) return;
            
            for (int i = 0; i < 5; i++)
            {
                string actionName = $"Ability{i + 1}";
                var abilityAction = playerActionMap.FindAction(actionName);
                if (abilityAction != null)
                {
                    _abilitySlotSystem.SetSlotInputAction(i, abilityAction);
                }
                else
                {
                    Debug.LogWarning($"[PlayerController] Не найден {actionName}");
                }
            }
        }
        
        public void LoadFromCharacterData(CharacterData characterData)
        {
            if (characterData == null)
            {
                return;
            }
            
            if (characterData.PlayerClass)
            {
                SetClass(characterData.PlayerClass);
            }
            
            if (_healthSystem)
            {
                _healthSystem.SetMaxHealth(characterData.MaxHealth);
                _healthSystem.SetMaxEnergy(characterData.MaxEnergy);
            }
        }
        
        public void SaveToCharacterData(CharacterData characterData)
        {
            if (characterData == null)
            {
                return;
            }

            if (_healthSystem)
            {
                characterData.CurrentHealth = _healthSystem.GetHealth();
                characterData.MaxHealth = _healthSystem.GetMaxHealth();
                characterData.CurrentEnergy = _healthSystem.CurrentEnergy;
                characterData.MaxEnergy = _healthSystem.MaxEnergy;
            }
        }
        
        private void AimTargetFollow()
        {
            Ray desiredTargetRay = _mainCamera.ScreenPointToRay(new Vector2(Screen.width/2, Screen.height/2));
            Vector3 desiredTargetPos = desiredTargetRay.origin + desiredTargetRay.direction * 0.7f;
            aimTarget.position = desiredTargetPos;
        }
    }
}

