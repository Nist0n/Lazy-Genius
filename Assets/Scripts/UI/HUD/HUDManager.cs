using UnityEngine;
using Player;
using System.Collections;
using Enemy;
using TMPro;

namespace UI.HUD
{
    public class HUDManager : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private ResourceBarUI healthBar;
        [SerializeField] private ResourceBarUI energyBar;
        [SerializeField] private CrosshairUI crosshair;
        [SerializeField] private TextMeshProUGUI killScoreText;
        
        [Header("Raycast Settings")]
        [SerializeField] private float interactionRange = 50f;
        [SerializeField] private LayerMask interactableLayers;
        
        private HealthSystem _playerHealth;
        private Camera _mainCamera;
        
        public CrosshairState CurrentCrosshairState { get; private set; } = CrosshairState.Normal;

        private void Update()
        {
            UpdateCrosshairRaycast();
        }

        public void Initialize(HealthSystem playerHealth, Camera camera)
        {
            if (!playerHealth)
            {
                return;
            }

            _mainCamera = camera ? camera : Camera.main;
            _playerHealth = playerHealth;

            _playerHealth.OnHealthChanged -= OnHealthChanged;
            _playerHealth.OnEnergyChanged -= OnEnergyChanged;
            _playerHealth.OnHealthChanged += OnHealthChanged;
            _playerHealth.OnEnergyChanged += OnEnergyChanged;

            OnHealthChanged(_playerHealth.GetHealth(), _playerHealth.GetMaxHealth());
            OnEnergyChanged(_playerHealth.CurrentEnergy, _playerHealth.MaxEnergy);
        }
        
        private void OnHealthChanged(float current, float max)
        {
            if (healthBar) healthBar.UpdateValue(current, max);
        }
        
        private void OnEnergyChanged(float current, float max)
        {
            if (energyBar) energyBar.UpdateValue(current, max);
        }

        public void OnKillScoreChanged(int score)
        {
            if (!killScoreText)
            {
                return;
            }

            killScoreText.text = score.ToString();
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
