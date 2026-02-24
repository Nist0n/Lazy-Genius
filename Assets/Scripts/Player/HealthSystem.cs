using System;
using Core;
using UnityEngine;

namespace Player
{
    public class HealthSystem : MonoBehaviour, IDamageable
    {
        [Header("Health Settings")]
        [SerializeField] private float currentHealth;
        [SerializeField] private float maxHealth;
        
        [Header("Energy Settings")]
        [SerializeField] private float currentEnergy;
        [SerializeField] private float maxEnergy;
        [SerializeField] private float energyRegenRate = 5f;
        [SerializeField] private float energyRegenDelay = 2f;
        
        private float _lastEnergyUseTime;
        private bool _isDead;
        
        public event Action<float, float> OnHealthChanged;
        public event Action<float, float> OnEnergyChanged;
        public event Action OnDeath;
        
        public float CurrentEnergy => currentEnergy;
        public float MaxEnergy => maxEnergy;
        public float HealthPercentage
        {
            get
            {
                if (maxHealth > 0) return currentHealth / maxHealth;
                else return 0f;
            }
        }

        public float EnergyPercentage
        {
            get
            {
                if (maxEnergy > 0) return currentEnergy / maxEnergy;
                else return 0f;
            }
        }

        public bool IsDead => _isDead;
        public bool HasEnergy(float amount) => currentEnergy >= amount;
        
        public float GetHealth() => currentHealth;
        public float GetMaxHealth() => maxHealth;
        
        private void Start()
        {
            Initialize();
        }
        
        private void Update()
        {
            RegenerateEnergy();
        }
        
        public void Initialize(float health = -1, float energy = -1)
        {
            if (health > 0) maxHealth = health;
            if (energy > 0) maxEnergy = energy;
            
            currentHealth = maxHealth;
            currentEnergy = maxEnergy;
            _isDead = false;
            
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
            OnEnergyChanged?.Invoke(currentEnergy, maxEnergy);
        }
        
        public void SetMaxHealth(float value)
        {
            float healthPercentage = HealthPercentage;
            maxHealth = value;
            currentHealth = maxHealth * healthPercentage;
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
        }
        
        public void SetMaxEnergy(float value)
        {
            float energyPercentage = EnergyPercentage;
            maxEnergy = value;
            currentEnergy = maxEnergy * energyPercentage;
            OnEnergyChanged?.Invoke(currentEnergy, maxEnergy);
        }

        public void TakeDamage(float damage, DamageInfo damageInfo)
        {
            if (_isDead) return;
            
            currentHealth = Mathf.Max(0, currentHealth - damage);
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
            
            Game.Events.GameEvents.OnEntityDamaged(gameObject, damage);
            
            if (currentHealth <= 0 && !_isDead)
            {
                Die();
            }
        }
        
        public bool UseEnergy(float amount)
        {
            if (currentEnergy < amount) return false;
            
            currentEnergy -= amount;
            _lastEnergyUseTime = Time.time;
            OnEnergyChanged?.Invoke(currentEnergy, maxEnergy);
            return true;
        }
        
        public void RestoreEnergy(float amount)
        {
            currentEnergy = Mathf.Min(maxEnergy, currentEnergy + amount);
            OnEnergyChanged?.Invoke(currentEnergy, maxEnergy);
        }
        
        private void RegenerateEnergy()
        {
            if (_isDead) return;
            if (Time.time - _lastEnergyUseTime < energyRegenDelay) return;
            if (currentEnergy >= maxEnergy) return;
            
            RestoreEnergy(energyRegenRate * Time.deltaTime);
        }
        
        private void Die()
        {
            _isDead = true;
            OnDeath?.Invoke();
        }
    }
}

