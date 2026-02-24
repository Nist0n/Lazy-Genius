using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Player
{
    public class PlayerInputHandler : MonoBehaviour
    {
        [Header("Input Settings")]
        [SerializeField] private InputActionAsset inputActions;
        
        private InputActionMap _playerActionMap;
        private InputAction _moveAction;
        private InputAction _lookAction;
        private InputAction _jumpAction;
        private InputAction _sprintAction;
        private InputAction _interactAction;
        private InputAction _ability1Action;
        private InputAction _ability2Action;
        private InputAction _ability3Action;
        private InputAction _ability4Action;
        private InputAction _ability5Action;
        
        private Vector2 _moveInput;
        private Vector2 _lookInput;
        private bool _jumpPressed;
        private bool _sprintPressed;
        
        public Action<Vector2> OnMoveInput;
        public Action<Vector2> OnLookInput;
        public Action OnJumpPressed;
        public Action OnJumpReleased;
        public Action OnSprintPressed;
        public Action OnSprintReleased;
        public Action OnInteractPressed;
        public Action OnAbility1Pressed;
        public Action OnAbility2Pressed;
        public Action OnAbility3Pressed;
        public Action OnAbility4Pressed;
        public Action OnAbility5Pressed;
        
        public Vector2 MoveInput => _moveInput;
        public Vector2 LookInput => _lookInput;
        public bool JumpPressed => _jumpPressed;
        public bool SprintPressed => _sprintPressed;
        
        private void Awake()
        {
            InitializeInput();
        }
        
        private void OnEnable()
        {
            EnableInput();
        }
        
        private void OnDisable()
        {
            DisableInput();
        }

        private void InitializeInput()
        {
            if (!inputActions)
            {
                return;
            }
            
            _playerActionMap = inputActions.FindActionMap("Player");
            if (_playerActionMap == null)
            {
                return;
            }
            
            _moveAction = _playerActionMap.FindAction("Move");
            _lookAction = _playerActionMap.FindAction("Look");
            _jumpAction = _playerActionMap.FindAction("Jump");
            _sprintAction = _playerActionMap.FindAction("Sprint");
            _interactAction = _playerActionMap.FindAction("Interact");
            
            if (_moveAction != null)
            {
                _moveAction.performed += OnMove;
                _moveAction.canceled += OnMove;
            }
            
            if (_lookAction != null)
            {
                _lookAction.performed += OnLook;
                _lookAction.canceled += OnLook;
            }
            
            if (_jumpAction != null)
            {
                _jumpAction.performed += OnJump;
                _jumpAction.canceled += JumpReleased;
            }
            
            if (_sprintAction != null)
            {
                _sprintAction.performed += OnSprint;
                _sprintAction.canceled += SprintReleased;
            }
            
            if (_interactAction != null)
            {
                _interactAction.performed += OnInteract;
            }
            
            _ability1Action = _playerActionMap.FindAction("Ability1");
            _ability2Action = _playerActionMap.FindAction("Ability2");
            _ability3Action = _playerActionMap.FindAction("Ability3");
            _ability4Action = _playerActionMap.FindAction("Ability4");
            _ability5Action = _playerActionMap.FindAction("Ability5");
            
            if (_ability1Action != null) _ability1Action.performed += OnAbility1;
            if (_ability2Action != null) _ability2Action.performed += OnAbility2;
            if (_ability3Action != null) _ability3Action.performed += OnAbility3;
            if (_ability4Action != null) _ability4Action.performed += OnAbility4;
            if (_ability5Action != null) _ability5Action.performed += OnAbility5;
        }
        
        private void EnableInput()
        {
            _playerActionMap?.Enable();
        }
        
        private void DisableInput()
        {
            _playerActionMap?.Disable();
        }
        
        private void OnMove(InputAction.CallbackContext context)
        {
            _moveInput = context.ReadValue<Vector2>();
            OnMoveInput?.Invoke(_moveInput);
        }
        
        private void OnLook(InputAction.CallbackContext context)
        {
            _lookInput = context.ReadValue<Vector2>();
            OnLookInput?.Invoke(_lookInput);
        }
        
        private void OnJump(InputAction.CallbackContext context)
        {
            _jumpPressed = true;
            OnJumpPressed?.Invoke();
        }
        
        private void JumpReleased(InputAction.CallbackContext context)
        {
            _jumpPressed = false;
            OnJumpReleased?.Invoke();
        }
        
        private void OnSprint(InputAction.CallbackContext context)
        {
            _sprintPressed = true;
            OnSprintPressed?.Invoke();
        }
        
        private void SprintReleased(InputAction.CallbackContext context)
        {
            _sprintPressed = false;
            OnSprintReleased?.Invoke();
        }
        
        private void OnInteract(InputAction.CallbackContext context)
        {
            OnInteractPressed?.Invoke();
        }
        
        private void OnAbility1(InputAction.CallbackContext context)
        {
            OnAbility1Pressed?.Invoke();
        }
        
        private void OnAbility2(InputAction.CallbackContext context)
        {
            OnAbility2Pressed?.Invoke();
        }
        
        private void OnAbility3(InputAction.CallbackContext context)
        {
            OnAbility3Pressed?.Invoke();
        }
        
        private void OnAbility4(InputAction.CallbackContext context)
        {
            OnAbility4Pressed?.Invoke();
        }
        
        private void OnAbility5(InputAction.CallbackContext context)
        {
            OnAbility5Pressed?.Invoke();
        }
        
        public InputActionAsset GetInputActionAsset()
        {
            return inputActions;
        }
        
        public InputActionMap GetPlayerActionMap()
        {
            return _playerActionMap;
        }
    }
}

