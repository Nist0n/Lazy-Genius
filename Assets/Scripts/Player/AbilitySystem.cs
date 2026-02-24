using System;
using System.Collections.Generic;
using UnityEngine;

namespace Player
{
    public class AbilitySystem : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private HealthSystem healthSystem;
        
        [Header("Ability Slots")]
        [SerializeField] private List<Ability> abilities = new List<Ability>();
        
        private Dictionary<int, float> _abilityCooldowns = new Dictionary<int, float>();
        private Dictionary<int, float> _abilityCooldownTimers = new Dictionary<int, float>();
        private Dictionary<int, bool> _activeChanneledAbilities = new Dictionary<int, bool>();
        private List<int> _cooldownKeysCache = new List<int>();
        
        private void Update()
        {
            UpdateCooldowns();
        }
        
        public void Initialize(HealthSystem health)
        {
            healthSystem = health;
            
            for (int i = 0; i < abilities.Count; i++)
            {
                if (abilities[i])
                {
                    _abilityCooldowns[i] = abilities[i].cooldown;
                    _abilityCooldownTimers[i] = 0f;
                }
            }
        }

        public bool UseAbility(int abilityIndex)
        {
            if (abilityIndex < 0 || abilityIndex >= abilities.Count)
            {
                return false;
            }
            
            if (!abilities[abilityIndex])
            {
                return false;
            }
            
            Ability ability = abilities[abilityIndex];
            
            if (ability.isChanneled)
            {
                if (_activeChanneledAbilities.ContainsKey(abilityIndex) && _activeChanneledAbilities[abilityIndex])
                {
                    DeactivateAbility(abilityIndex);
                    return true;
                }
                
                if (IsAbilityOnCooldown(abilityIndex))
                {
                    return false;
                }
            }
            else
            {
                if (IsAbilityOnCooldown(abilityIndex))
                {
                    return false;
                }
            }
            
            if (!CanUseAbility(abilityIndex))
            {
                return false;
            }
            
            if (healthSystem && !healthSystem.HasEnergy(ability.energyCost))
            {
                return false;
            }
            
            if (!ability.isChanneled && healthSystem)
            {
                healthSystem.UseEnergy(ability.energyCost);
            }
            
            ability.Activate(gameObject);
            
            if (ability.isChanneled)
            {
                _activeChanneledAbilities[abilityIndex] = true;
            }
            else if (ability.cooldown > 0)
            {
                _abilityCooldownTimers[abilityIndex] = ability.cooldown;
            }
            return true;
        }
        
        public bool DeactivateAbility(int abilityIndex)
        {
            if (abilityIndex < 0 || abilityIndex >= abilities.Count) return false;
            if (!abilities[abilityIndex]) return false;
            
            Ability ability = abilities[abilityIndex];
            
            if (ability.isChanneled && _activeChanneledAbilities.ContainsKey(abilityIndex) && _activeChanneledAbilities[abilityIndex])
            {
                ability.Deactivate();
                _activeChanneledAbilities[abilityIndex] = false;
                
                if (ability.cooldown > 0)
                {
                    _abilityCooldownTimers[abilityIndex] = ability.cooldown;
                }
                return true;
            }
            
            return false;
        }
        
        public bool IsChanneledAbilityActive(int abilityIndex)
        {
            if (!_activeChanneledAbilities.ContainsKey(abilityIndex)) return false;
            return _activeChanneledAbilities[abilityIndex];
        }
        
        private bool CanUseAbility(int abilityIndex)
        {
            if (abilityIndex < 0 || abilityIndex >= abilities.Count) return false;
            if (!abilities[abilityIndex]) return false;
            
            return abilities[abilityIndex].CanActivate();
        }
        
        public bool IsAbilityOnCooldown(int abilityIndex)
        {
            if (!_abilityCooldownTimers.ContainsKey(abilityIndex)) return false;
            return _abilityCooldownTimers[abilityIndex] > 0f;
        }
        
        public float GetCooldownRemaining(int abilityIndex)
        {
            if (!_abilityCooldownTimers.ContainsKey(abilityIndex)) return 0f;
            return _abilityCooldownTimers[abilityIndex];
        }
        
        public float GetCooldownPercentage(int abilityIndex)
        {
            if (!_abilityCooldowns.ContainsKey(abilityIndex)) return 0f;
            float cooldown = _abilityCooldowns[abilityIndex];
            if (cooldown <= 0) return 0f;
            return _abilityCooldownTimers.ContainsKey(abilityIndex) ? 
                _abilityCooldownTimers[abilityIndex] / cooldown : 0f;
        }
        
        private void UpdateCooldowns()
        {
            _cooldownKeysCache.Clear();
            _cooldownKeysCache.AddRange(_abilityCooldownTimers.Keys);
            
            foreach (var key in _cooldownKeysCache)
            {
                if (_abilityCooldownTimers[key] > 0f)
                {
                    _abilityCooldownTimers[key] = Mathf.Max(0f, _abilityCooldownTimers[key] - Time.deltaTime);
                }
            }
        }

        public void AddAbility(Ability ability, int slotIndex = -1)
        {
            if (slotIndex < 0)
            {
                abilities.Add(ability);
                slotIndex = abilities.Count - 1;
            }
            else
            {
                if (slotIndex >= abilities.Count)
                {
                    while (abilities.Count <= slotIndex)
                    {
                        abilities.Add(null);
                    }
                }
                abilities[slotIndex] = ability;
            }
            
            _abilityCooldowns[slotIndex] = ability.cooldown;
            _abilityCooldownTimers[slotIndex] = 0f;
        }
        
        public void RemoveAbility(int abilityIndex)
        {
            if (abilityIndex >= 0 && abilityIndex < abilities.Count)
            {
                abilities[abilityIndex] = null;
                _abilityCooldowns.Remove(abilityIndex);
                _abilityCooldownTimers.Remove(abilityIndex);
            }
        }
    }
    
    [Serializable]
    public abstract class Ability : ScriptableObject
    {
        [Header("Ability Info")]
        public string abilityName;
        public Sprite icon;
        
        [Header("Ability Settings")]
        public float cooldown = 1f;
        public float energyCost = 10f;
        [Tooltip("Если true, умение активируется при зажатии кнопки и деактивируется при отпускании")]
        public bool isChanneled = false;
        
        public abstract void Activate(GameObject caster);
        
        public virtual bool CanActivate()
        {
            return true;
        }
        
        public virtual void Deactivate()
        {
            
        }
    }
}

