using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Enemy;
using Enemy.Boss;

namespace UI.Enemy
{
    public class EnemyHealthBar : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Image healthFillImage;
        [SerializeField] private Canvas canvas;
        [SerializeField] private float visibleTimeAfterDamage = 3f;
        
        private Camera _mainCamera;
        private Coroutine _hideCoroutine;
        private bool _isHovered;
        private float _lastDamageTime;
        private EnemyHealth _targetHealth;
        private BossController _bossController;

        private void Awake()
        {
            _mainCamera = Camera.main;
        }

        private void Start()
        {
            if (canvas) canvas.enabled = false;
            
            _targetHealth = GetComponentInParent<EnemyHealth>();
            if (_targetHealth)
            {
                _targetHealth.OnHealthChanged += OnHealthChanged;
                UpdateHealthBar(_targetHealth.CurrentHealth, _targetHealth.MaxHealth);
            }
            else
            {
                _bossController = GetComponentInParent<BossController>();
                
                if (_bossController)
                {
                    Debug.Log(_bossController);
                    _bossController.OnHealthChanged += OnHealthChanged;
                    UpdateHealthBar(_bossController.GetHealth(), _bossController.GetMaxHealth());
                }
            }
        }
        
        private void LateUpdate()
        {
            if (canvas && canvas.enabled && _mainCamera)
            {
                transform.LookAt(_mainCamera.transform.position);
            }
        }
        
        private void OnHealthChanged(float currentHealth)
        {
            if (_targetHealth)
            {
                UpdateHealthBar(currentHealth, _targetHealth.MaxHealth);
            }

            if (_bossController)
            {
                UpdateHealthBar(currentHealth, _bossController.GetMaxHealth());
            }
        }
        
        public void UpdateHealthBar(float current, float max)
        {
            if (healthFillImage && max > 0)
            {
                healthFillImage.fillAmount = current / max;
            }
        }
        
        public void OnDamageTaken()
        {
            _lastDamageTime = Time.time;
            Show();
            
            if (_hideCoroutine != null) StopCoroutine(_hideCoroutine);
            _hideCoroutine = StartCoroutine(HideAfterDelay());
        }
        
        public void SetHovered(bool hovered)
        {
            if (_isHovered == hovered) return;
            
            _isHovered = hovered;
            
            if (_isHovered)
            {
                Show();
                if (_hideCoroutine != null) StopCoroutine(_hideCoroutine);
            }
            else
            {
                if (Time.time - _lastDamageTime < visibleTimeAfterDamage)
                {
                    if (_hideCoroutine != null) StopCoroutine(_hideCoroutine);
                    _hideCoroutine = StartCoroutine(HideAfterDelay());
                }
                else
                {
                    Hide();
                }
            }
        }
        
        public void Show()
        {
            if (canvas)
            {
                if (!canvas.enabled && _targetHealth)
                {
                    UpdateHealthBar(_targetHealth.CurrentHealth, _targetHealth.MaxHealth);
                }
                
                if (!canvas.enabled && _bossController)
                {
                    UpdateHealthBar(_bossController.GetHealth(), _bossController.GetMaxHealth());
                }
                
                canvas.enabled = true;
            }
        }
        
        public void Hide()
        {
            if (canvas) canvas.enabled = false;
        }
        
        private IEnumerator HideAfterDelay()
        {
            float remainingTime = visibleTimeAfterDamage - (Time.time - _lastDamageTime);
            if (remainingTime > 0)
            {
                yield return new WaitForSeconds(remainingTime);
            }
            
            if (!_isHovered)
            {
                Hide();
            }
            _hideCoroutine = null;
        }
        
        private void OnDestroy()
        {
            if (_targetHealth)
            {
                _targetHealth.OnHealthChanged -= OnHealthChanged;
            }

            if (_bossController)
            {
                _bossController.OnHealthChanged -= OnHealthChanged;
            }
        }
    }
}
