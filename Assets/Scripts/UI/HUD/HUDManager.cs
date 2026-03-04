using UnityEngine;
using Player;
using System.Collections;
using Enemy;

namespace UI.HUD
{
    public class HUDManager : MonoBehaviour
    {
        public static HUDManager Instance { get; private set; }

        [Header("Components")]
        [SerializeField] private ResourceBarUI healthBar;
        [SerializeField] private ResourceBarUI energyBar;
        [SerializeField] private CrosshairUI crosshair;
        
        [Header("Raycast Settings")]
        [SerializeField] private float interactionRange = 50f;
        [SerializeField] private LayerMask interactableLayers;
        
        private HealthSystem _playerHealth;
        private Camera _mainCamera;
        
        public CrosshairState CurrentCrosshairState { get; private set; } = CrosshairState.Normal;
        
        private void Awake()
        {
            if (Instance && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            _mainCamera = Camera.main;
            InitializePlayer();
        }

        private void Update()
        {
            UpdateCrosshairRaycast();
        }

        public void InitializePlayer()
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player)
            {
                _playerHealth = player.GetComponent<HealthSystem>();
                if (_playerHealth)
                {
                    _playerHealth.OnHealthChanged -= OnHealthChanged;
                    _playerHealth.OnEnergyChanged -= OnEnergyChanged;
                    
                    _playerHealth.OnHealthChanged += OnHealthChanged;
                    _playerHealth.OnEnergyChanged += OnEnergyChanged;
                    
                    OnHealthChanged(_playerHealth.GetHealth(), _playerHealth.GetMaxHealth());
                    OnEnergyChanged(_playerHealth.CurrentEnergy, _playerHealth.MaxEnergy);
                }
            }
        }
        
        private void OnHealthChanged(float current, float max)
        {
            if (healthBar) healthBar.UpdateValue(current, max);
        }
        
        private void OnEnergyChanged(float current, float max)
        {
            if (energyBar) energyBar.UpdateValue(current, max);
        }

        private void UpdateCrosshairRaycast()
        {
            if (!_mainCamera || !crosshair) return;

            Ray ray = _mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
            RaycastHit hit;

            bool hitSomething = Physics.Raycast(ray, out hit, interactionRange, interactableLayers);
            
            CrosshairState targetState = CrosshairState.Normal;
            
            if (hitSomething)
            {
                if (hit.collider.CompareTag("Enemy") || hit.collider.GetComponentInParent<EnemyController>())
                {
                    targetState = CrosshairState.Enemy;
                }
            }
            
            if (targetState != CurrentCrosshairState)
            {
                CurrentCrosshairState = targetState;
                crosshair.SetState(targetState);
            }
        }
        
        private void OnDestroy()
        {
            if (_playerHealth)
            {
                _playerHealth.OnHealthChanged -= OnHealthChanged;
                _playerHealth.OnEnergyChanged -= OnEnergyChanged;
            }
        }
    }
}
