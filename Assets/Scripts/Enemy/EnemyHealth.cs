using UnityEngine;
using Core;
using System;
using System.Collections;
using Audio;
using Game.Events;

namespace Enemy
{
    public class EnemyHealth : MonoBehaviour, IDamageable
    {
        public event Action<float> OnHealthChanged;
        public event Action<DamageInfo> OnDamageTaken;

        [SerializeField] private float currentHealth;
        [SerializeField] private float maxHealth = 100f;
        [SerializeField] private float deathAnimationDuration = 1f;

        private bool _isDead;
        private EnemyController _enemyController;
        private EventBus _eventBus;
        
        public float CurrentHealth => currentHealth;
        public float MaxHealth => maxHealth;
        public bool IsDead => _isDead;
        public float GetHealth() => currentHealth;
        public float GetMaxHealth() => maxHealth;

        private void Awake()
        {
            _enemyController = GetComponent<EnemyController>();
        }

        public void Initialize(float health)
        {
            maxHealth = health;
            currentHealth = health;
            _isDead = false;
        }

        public void BindEventBus(EventBus eventBus)
        {
            _eventBus = eventBus;
        }

        public void TakeDamage(float damage, DamageInfo info)
        {
            if (_isDead) return;
            
            AudioManager.Instance.PlayLocalSound("EnemyHit", _enemyController.SoundSource);
            
            currentHealth -= damage;
            
            OnHealthChanged?.Invoke(currentHealth);
            Debug.Log("GetHitReleased");
            OnDamageTaken?.Invoke(info);
            
            _eventBus?.Publish(new EntityDamagedEvent(gameObject, damage));

            if (currentHealth <= 0)
            {
                Die();
            }
        }

        private void Die()
        {
            if (_isDead) return;
            
            AudioManager.Instance.PlayLocalSound("EnemyDeath", _enemyController.SoundSource);
            
            _isDead = true;
            currentHealth = 0;

            _eventBus?.Publish(new MobKilledEvent(gameObject));
            
            if (_enemyController)
            {
                if (_enemyController.Agent)
                {
                    _enemyController.OnDeath();
                    _enemyController.enabled = false;
                }
            }
            
            StartCoroutine(PlayDeathAnimationAndDestroy());
        }

        private IEnumerator PlayDeathAnimationAndDestroy()
        {
            yield return new WaitForSeconds(deathAnimationDuration);
            
            Destroy(gameObject);
        }
    }
}
