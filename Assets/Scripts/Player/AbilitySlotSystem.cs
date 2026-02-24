using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Player
{
    public class AbilitySlotSystem : MonoBehaviour
    {
        [Header("Slot Configuration")]
        [SerializeField] private int maxSlots = 5;
        
        [Header("Available Abilities")]
        [SerializeField] private List<Ability> availableAbilities = new List<Ability>();
        
        [Header("Assigned Slots")]
        [SerializeField] private List<Ability> assignedAbilities = new List<Ability>();
        
        [Header("Slot Key Bindings")]
        [SerializeField] private List<KeyCode> slotKeyBindings = new List<KeyCode>();
        
        [Header("Slot Input Action Bindings")]
        [SerializeField] private List<string> slotInputActionNames = new List<string>();
        
        private AbilitySystem _abilitySystem;
        private Dictionary<int, InputAction> _slotInputActions = new Dictionary<int, InputAction>();
        
        public event Action<int, Ability> OnAbilityAssignedToSlot;
        public event Action<int> OnAbilityRemovedFromSlot;
        public event Action<int> OnSlotInputActionChanged;
        
        public int MaxSlots => maxSlots;
        
        private void Awake()
        {
            _abilitySystem = GetComponent<AbilitySystem>();
            InitializeSlots();
        }
        
        private void InitializeSlots()
        {
            while (assignedAbilities.Count < maxSlots)
            {
                assignedAbilities.Add(null);
            }
            
            if (slotKeyBindings.Count < maxSlots)
            {
                KeyCode[] defaultKeys = { KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3, KeyCode.Alpha4, KeyCode.Alpha5 };
                while (slotKeyBindings.Count < maxSlots)
                {
                    int index = slotKeyBindings.Count;
                    if (index < defaultKeys.Length) slotKeyBindings.Add(defaultKeys[index]);
                    else slotKeyBindings.Add(KeyCode.None);
                }
            }
            
            if (slotInputActionNames.Count < maxSlots)
            {
                string[] defaultActions = { "Ability1", "Ability2", "Ability3", "Ability4", "Ability5" };
                while (slotInputActionNames.Count < maxSlots)
                {
                    int index = slotInputActionNames.Count;
                    slotInputActionNames.Add(index < defaultActions.Length ? defaultActions[index] : "");
                }
            }
        }
        
        
        public void AddAvailableAbility(Ability ability)
        {
            if (ability && !availableAbilities.Contains(ability))
            {
                availableAbilities.Add(ability);
            }
        }
        
        public bool AssignAbilityToSlot(Ability ability, int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= maxSlots)
            {
                return false;
            }
            
            if (ability && !availableAbilities.Contains(ability))
            {
                return false;
            }
            
            assignedAbilities[slotIndex] = ability;
            
            if (_abilitySystem)
            {
                if (ability)
                {
                    _abilitySystem.AddAbility(ability, slotIndex);
                }
                else
                {
                    _abilitySystem.RemoveAbility(slotIndex);
                }
            }
            
            OnAbilityAssignedToSlot?.Invoke(slotIndex, ability);
            return true;
        }
        
        public void RemoveAbilityFromSlot(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= maxSlots) return;
            
            assignedAbilities[slotIndex] = null;
            
            if (_abilitySystem)
            {
                _abilitySystem.RemoveAbility(slotIndex);
            }
            
            OnAbilityRemovedFromSlot?.Invoke(slotIndex);
        }
        
        public Ability GetAbilityFromSlot(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= maxSlots) return null;
            return assignedAbilities[slotIndex];
        }

        public KeyCode GetSlotKeyBinding(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= maxSlots) return KeyCode.None;
            if (slotIndex >= slotKeyBindings.Count) return KeyCode.None;
            return slotKeyBindings[slotIndex];
        }
        
        public void SetSlotInputAction(int slotIndex, InputAction inputAction)
        {
            if (slotIndex < 0 || slotIndex >= maxSlots) return;
            
            if (_slotInputActions.ContainsKey(slotIndex))
            {
                var oldAction = _slotInputActions[slotIndex];
                if (oldAction != null)
                {
                    oldAction.performed -= OnSlotInputPerformed;
                    oldAction.canceled -= OnSlotInputCanceled;
                }
            }
            
            if (inputAction != null)
            {
                _slotInputActions[slotIndex] = inputAction;
                inputAction.performed += OnSlotInputPerformed;
                inputAction.canceled += OnSlotInputCanceled;
                
                while (slotInputActionNames.Count <= slotIndex)
                {
                    slotInputActionNames.Add("");
                }
                slotInputActionNames[slotIndex] = inputAction.name;
            }
            else
            {
                if (_slotInputActions.ContainsKey(slotIndex))
                {
                    _slotInputActions.Remove(slotIndex);
                }
                while (slotInputActionNames.Count <= slotIndex)
                {
                    slotInputActionNames.Add("");
                }
                slotInputActionNames[slotIndex] = "";
            }
            
            OnSlotInputActionChanged?.Invoke(slotIndex);
        }
        
        public InputAction GetSlotInputAction(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= maxSlots) return null;
            if (_slotInputActions.ContainsKey(slotIndex))
            {
                return _slotInputActions[slotIndex];
            }
            return null;
        }
        
        private void OnSlotInputPerformed(InputAction.CallbackContext context)
        {
            if (Time.timeScale == 0f) return;

            foreach (var kvp in _slotInputActions)
            {
                if (kvp.Value == context.action)
                {
                    int slotIndex = kvp.Key;
                    var ability = GetAbilityFromSlot(slotIndex);
                    if (ability)
                    {
                        UseAbilityFromSlot(slotIndex);
                    }
                    break;
                }
            }
        }
        
        private void OnSlotInputCanceled(InputAction.CallbackContext context)
        {
            foreach (var kvp in _slotInputActions)
            {
                if (kvp.Value == context.action)
                {
                    int slotIndex = kvp.Key;
                    var ability = GetAbilityFromSlot(slotIndex);
                    if (ability && ability.isChanneled && _abilitySystem)
                    {
                        if (_abilitySystem.IsChanneledAbilityActive(slotIndex))
                        {
                            _abilitySystem.DeactivateAbility(slotIndex);
                        }
                    }
                    break;
                }
            }
        }
        
        public string GetSlotBindingDisplayString(int slotIndex)
        {
            var inputAction = GetSlotInputAction(slotIndex);
            if (inputAction != null)
            {
                var bindings = inputAction.bindings;
                for (int i = 0; i < bindings.Count; i++)
                {
                    var binding = bindings[i];
                    if (binding.isComposite) continue;
                    
                    if (binding.overridePath != null && !string.IsNullOrEmpty(binding.overridePath))
                    {
                        string displayString = inputAction.GetBindingDisplayString(i, InputBinding.DisplayStringOptions.DontUseShortDisplayNames);
                        if (!string.IsNullOrEmpty(displayString))
                        {
                            return displayString;
                        }
                    }
                    else if (binding.effectivePath != null && !string.IsNullOrEmpty(binding.effectivePath))
                    {
                        string displayString = inputAction.GetBindingDisplayString(i, InputBinding.DisplayStringOptions.DontUseShortDisplayNames);
                        if (!string.IsNullOrEmpty(displayString))
                        {
                            return displayString;
                        }
                    }
                }
                
                string fullDisplayString = inputAction.GetBindingDisplayString(InputBinding.DisplayStringOptions.DontUseShortDisplayNames);
                if (!string.IsNullOrEmpty(fullDisplayString))
                {
                    int separatorIndex = fullDisplayString.IndexOf('|');
                    if (separatorIndex > 0)
                    {
                        return fullDisplayString.Substring(0, separatorIndex).Trim();
                    }
                    return fullDisplayString;
                }
            }
            return "";
        }
        
        public void NotifyBindingChanged(int slotIndex)
        {
            OnSlotInputActionChanged?.Invoke(slotIndex);
        }
        
        public bool UseAbilityFromSlot(int slotIndex)
        {
            if (!_abilitySystem)
            {
                return false;
            }
            
            var ability = GetAbilityFromSlot(slotIndex);
            if (!ability)
            {
                return false;
            }
            
            return _abilitySystem.UseAbility(slotIndex);
        }
        
        private void OnDestroy()
        {
            foreach (var kvp in _slotInputActions)
            {
                if (kvp.Value != null)
                {
                    kvp.Value.performed -= OnSlotInputPerformed;
                    kvp.Value.canceled -= OnSlotInputCanceled;
                }
            }
            _slotInputActions.Clear();
        }
    }
}

