using System.Collections.Generic;
using UnityEngine;

namespace Player
{
    public abstract class PlayerClass : ScriptableObject
    {
        [Header("Class Info")]
        [SerializeField] protected string className;
        [SerializeField] protected string description;
        [SerializeField] protected Sprite classIcon;
        
        [Header("Class Stats Modifiers")]
        [SerializeField] protected float healthModifier = 0f;
        [SerializeField] protected float energyModifier = 0f;
        
        [Header("Class Abilities")]
        [SerializeField] protected List<Ability> classAbilities = new List<Ability>();
        
        public string ClassName => className;
        public string Description => description;
        public Sprite ClassIcon => classIcon;
        
        public virtual void Initialize(PlayerController player)
        {
            ApplyStatModifiers(player);
            
            SetupAbilities(player);
        }
        
        protected virtual void ApplyStatModifiers(PlayerController player)
        {
            HealthSystem healthSystem = player.GetComponent<HealthSystem>();
            if (healthSystem)
            {
                if (healthModifier != 0f)
                {
                    float currentMaxHealth = healthSystem.GetMaxHealth();
                    healthSystem.SetMaxHealth(currentMaxHealth * (1f + healthModifier));
                }
                
                if (energyModifier != 0f)
                {
                    float currentMaxEnergy = healthSystem.MaxEnergy;
                    healthSystem.SetMaxEnergy(currentMaxEnergy * (1f + energyModifier));
                }
            }
        }
        
        protected virtual void SetupAbilities(PlayerController player)
        {
            AbilitySystem abilitySystem = player.GetComponent<AbilitySystem>();
            if (abilitySystem)
            {
                for (int i = 0; i < classAbilities.Count; i++)
                {
                    if (classAbilities[i])
                    {
                        abilitySystem.AddAbility(classAbilities[i], i);
                    }
                }
            }
        }
        
        public virtual void OnUpdate(PlayerController player) {}
    }
}

