using System;
using System.Collections.Generic;
using UnityEngine;

namespace Player.Settings
{
    [CreateAssetMenu(fileName = "KeyBindingSettings", menuName = "Lazy-Genius/Player/Settings/KeyBindingSettings")]
    public class KeyBindingSettings : ScriptableObject
    {
        [Serializable]
        public class SlotBinding
        {
            public int slotIndex;
            public string inputActionName;
            public int bindingIndex = 0; // Индекс привязки в InputAction (если несколько)
            
            public SlotBinding() { }
            
            public SlotBinding(int slot, string actionName, int bindingIdx = 0)
            {
                slotIndex = slot;
                inputActionName = actionName;
                bindingIndex = bindingIdx;
            }
        }
        
        [Header("Default Slot Bindings")]
        [SerializeField] private List<SlotBinding> defaultBindings = new List<SlotBinding>();
        
        [Header("Current Slot Bindings")]
        [SerializeField] private List<SlotBinding> currentBindings = new List<SlotBinding>();
        
        public List<SlotBinding> DefaultBindings => defaultBindings;
        public List<SlotBinding> CurrentBindings => currentBindings;
        
        public event Action<int, string> OnBindingChanged;
        
        public void InitializeDefaults(int maxSlots)
        {
            if (defaultBindings.Count == 0)
            {
                defaultBindings = new List<SlotBinding>
                {
                    new SlotBinding(0, "Ability1", 0),
                    new SlotBinding(1, "Ability2", 0),
                    new SlotBinding(2, "Ability3", 0),
                    new SlotBinding(3, "Ability4", 0),
                    new SlotBinding(4, "Ability5", 0),
                    new SlotBinding(5, "Interact", 0),
                    new SlotBinding(6, "Sprint", 0)
                };
            }
            
            if (currentBindings.Count == 0 || currentBindings.Count != defaultBindings.Count)
            {
                currentBindings = new List<SlotBinding>();
                foreach (var binding in defaultBindings)
                {
                    currentBindings.Add(new SlotBinding(binding.slotIndex, binding.inputActionName, binding.bindingIndex));
                }
            }
        }
        
        public void SetSlotBinding(int slotIndex, string inputActionName, int bindingIndex = 0)
        {
            var existing = currentBindings.Find(b => b.slotIndex == slotIndex);
            if (existing != null)
            {
                existing.inputActionName = inputActionName;
                existing.bindingIndex = bindingIndex;
            }
            else
            {
                currentBindings.Add(new SlotBinding(slotIndex, inputActionName, bindingIndex));
            }
            
            OnBindingChanged?.Invoke(slotIndex, inputActionName);
        }
        
        public SlotBinding GetSlotBinding(int slotIndex)
        {
            return currentBindings.Find(b => b.slotIndex == slotIndex);
        }
        
        public void ResetSlotToDefault(int slotIndex)
        {
            var defaultBinding = defaultBindings.Find(b => b.slotIndex == slotIndex);
            if (defaultBinding != null)
            {
                SetSlotBinding(slotIndex, defaultBinding.inputActionName, defaultBinding.bindingIndex);
            }
        }
    }
}

