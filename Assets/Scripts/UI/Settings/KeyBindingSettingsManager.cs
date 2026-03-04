using System;
using System.Collections.Generic;
using Player;
using Player.Settings;
using UnityEngine;
using UnityEngine.InputSystem;

namespace UI.Settings
{
    public class KeyBindingSettingsManager : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private KeyBindingSettings keyBindingSettings;
        
        [Header("Input System")]
        [SerializeField] private InputActionAsset inputActionAsset;
        
        private AbilitySlotSystem _abilitySlotSystem;
        private PlayerInputHandler _inputHandler;
        
        public KeyBindingSettings Settings => keyBindingSettings;
        public InputActionAsset InputActionAsset => inputActionAsset;
        
        public event Action<int, string> OnBindingChanged;
        
        private void Awake()
        {
            if (!keyBindingSettings)
            {
                Debug.LogError("[KeyBindingSettingsManager] KeyBindingSettings отсутствует");
                return;
            }
            
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player)
            {
                _abilitySlotSystem = player.GetComponent<AbilitySlotSystem>();
                _inputHandler = player.GetComponent<PlayerInputHandler>();
                
                if (_inputHandler && !inputActionAsset)
                {
                    inputActionAsset = _inputHandler.GetInputActionAsset();
                }
            }
            
            if (!inputActionAsset)
            {
                var inputAsset = FindAnyObjectByType<PlayerInputHandler>();
                if (inputAsset)
                {
                    inputActionAsset = inputAsset.GetInputActionAsset();
                }
            }
            
            if (keyBindingSettings)
            {
                keyBindingSettings.OnBindingChanged += OnSettingsBindingChanged;
            }
        }
        
        private void Start()
        {
            LoadSettings();
            LoadBindingOverrides();
            ApplySettings();
        }
        
        private void LoadBindingOverrides()
        {
            if (!inputActionAsset) return;
            
            var playerActionMap = inputActionAsset.FindActionMap("Player");
            if (playerActionMap == null) return;
            
            foreach (var action in playerActionMap.actions)
            {
                string key = $"InputRebind_{playerActionMap.name}_{action.name}";
                if (PlayerPrefs.HasKey(key))
                {
                    string overridesJson = PlayerPrefs.GetString(key);
                    if (!string.IsNullOrEmpty(overridesJson))
                    {
                        action.LoadBindingOverridesFromJson(overridesJson);
                        Debug.Log($"[KeyBindingSettingsManager] Загружены переопределения для {action.name}");
                    }
                }
            }
        }
        
        private void OnDestroy()
        {
            if (keyBindingSettings)
            {
                keyBindingSettings.OnBindingChanged -= OnSettingsBindingChanged;
            }
        }
        
        private void OnSettingsBindingChanged(int slotIndex, string actionName)
        {
            ApplySlotBinding(slotIndex);
            OnBindingChanged?.Invoke(slotIndex, actionName);
        }
        
        public void ApplySettings()
        {
            if (!inputActionAsset) return;
            
            var playerActionMap = inputActionAsset.FindActionMap("Player");
            if (playerActionMap == null) return;
            
            if (!keyBindingSettings) return;

            int maxSlots;
            if (_abilitySlotSystem) maxSlots = _abilitySlotSystem.MaxSlots;
            else maxSlots = 5;
            keyBindingSettings.InitializeDefaults(maxSlots);
            
            if (_abilitySlotSystem)
            {
                foreach (var binding in keyBindingSettings.CurrentBindings)
                {
                    ApplySlotBinding(binding.slotIndex);
                }
            }

            for (int i = 0; i < keyBindingSettings.CurrentBindings.Count; i++)
            {
                SetSlotBindingByInputAction(i, playerActionMap.FindAction(keyBindingSettings.CurrentBindings[i].inputActionName));
            }
        }
        
        private void ApplySlotBinding(int slotIndex)
        {
            if (!inputActionAsset) return;
            
            var binding = keyBindingSettings.GetSlotBinding(slotIndex);
            if (binding == null) return;
            
            var playerActionMap = inputActionAsset.FindActionMap("Player");
            if (playerActionMap == null) return;
            
            var action = playerActionMap.FindAction(binding.inputActionName);
            
            if (action != null)
            {
                if (_abilitySlotSystem)
                {
                    _abilitySlotSystem.SetSlotInputAction(slotIndex, action);
                }
            }
            else
            {
                Debug.LogWarning($"[KeyBindingSettingsManager] InputAction '{binding.inputActionName}' не найден!");
            }
        }
        
        public void SetSlotBinding(int slotIndex, string inputActionName, int bindingIndex = 0)
        {
            if (!keyBindingSettings) return;
            
            keyBindingSettings.SetSlotBinding(slotIndex, inputActionName, bindingIndex);
            SaveSettings();
        }
        
        public void SetSlotBindingByInputAction(int slotIndex, InputAction inputAction, int bindingIndex = 0)
        {
            if (inputAction == null) return;
            
            SetSlotBinding(slotIndex, inputAction.name, bindingIndex);
        }
        
        public void SaveSettings()
        {
            if (!keyBindingSettings) return;
            
            for (int i = 0; i < keyBindingSettings.CurrentBindings.Count; i++)
            {
                var binding = keyBindingSettings.CurrentBindings[i];
                PlayerPrefs.SetString($"KeyBinding_Slot_{binding.slotIndex}_Action", binding.inputActionName);
                PlayerPrefs.SetInt($"KeyBinding_Slot_{binding.slotIndex}_BindingIndex", binding.bindingIndex);
            }
            
            PlayerPrefs.SetInt("KeyBinding_SettingsSaved", 1);
            PlayerPrefs.Save();
        }
        
        public void LoadSettings()
        {
            if (!keyBindingSettings) return;
            
            if (PlayerPrefs.GetInt("KeyBinding_SettingsSaved", 0) == 0)
            {
                Debug.Log("[KeyBindingSettingsManager] Используем значения по умолчанию");
                if (_abilitySlotSystem) keyBindingSettings.InitializeDefaults(_abilitySlotSystem.MaxSlots);
                else keyBindingSettings.InitializeDefaults(5);
                return;
            }

            if (_abilitySlotSystem) keyBindingSettings.InitializeDefaults(_abilitySlotSystem.MaxSlots);
            else keyBindingSettings.InitializeDefaults(5);

            for (int i = 0; i < 5; i++)
            {
                string actionName = PlayerPrefs.GetString($"KeyBinding_Slot_{i}_Action", "");
                int bindingIndex = PlayerPrefs.GetInt($"KeyBinding_Slot_{i}_BindingIndex", 0);
                
                if (!string.IsNullOrEmpty(actionName))
                {
                    keyBindingSettings.SetSlotBinding(i, actionName, bindingIndex);
                }
            }
        }
        
        public void ResetSlotToDefault(int slotIndex)
        {
            if (!keyBindingSettings) return;
            
            if (_abilitySlotSystem)
            {
                var action = _abilitySlotSystem.GetSlotInputAction(slotIndex);
                if (action != null)
                {
                    action.RemoveAllBindingOverrides();
                    
                    if (inputActionAsset)
                    {
                        var playerActionMap = inputActionAsset.FindActionMap("Player");
                        if (playerActionMap != null)
                        {
                            PlayerPrefs.DeleteKey($"InputRebind_{playerActionMap.name}_{action.name}");
                            PlayerPrefs.Save();
                        }
                    }
                }
            }
            
            keyBindingSettings.ResetSlotToDefault(slotIndex);
            SaveSettings();
            ApplySlotBinding(slotIndex);
        }
        
        public InputAction GetInputActionForSlot(int slotIndex)
        {
            if (!inputActionAsset) return null;
            
            var binding = keyBindingSettings.GetSlotBinding(slotIndex);
            if (binding == null) return null;
            
            var playerActionMap = inputActionAsset.FindActionMap("Player");
            if (playerActionMap == null) return null;
            
            var action = playerActionMap.FindAction(binding.inputActionName);
            
            return action;
        }
        
        public string GetSlotBindingDisplayString(int slotIndex)
        {
            var action = GetInputActionForSlot(slotIndex);
            if (action == null) return "Не назначено";
            
            var bindings = action.bindings;
            for (int i = 0; i < bindings.Count; i++)
            {
                var binding = bindings[i];
                if (binding.isComposite) continue;
                
                if (binding.overridePath != null && !string.IsNullOrEmpty(binding.overridePath))
                {
                    string displayString = action.GetBindingDisplayString(i, InputBinding.DisplayStringOptions.DontUseShortDisplayNames);
                    if (!string.IsNullOrEmpty(displayString))
                    {
                        return displayString;
                    }
                }
                else if (binding.effectivePath != null && !string.IsNullOrEmpty(binding.effectivePath))
                {
                    string displayString = action.GetBindingDisplayString(i, InputBinding.DisplayStringOptions.DontUseShortDisplayNames);
                    if (!string.IsNullOrEmpty(displayString))
                    {
                        return displayString;
                    }
                }
            }
            
            string fullDisplayString = action.GetBindingDisplayString(InputBinding.DisplayStringOptions.DontUseShortDisplayNames);
            if (!string.IsNullOrEmpty(fullDisplayString))
            {
                int separatorIndex = fullDisplayString.IndexOf('|');
                if (separatorIndex > 0)
                {
                    return fullDisplayString.Substring(0, separatorIndex).Trim();
                }
                return fullDisplayString;
            }
            
            return "";
        }
    }
}

