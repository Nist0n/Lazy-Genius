using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;
using Player.Settings;
using UI.Settings;

namespace Player.UI
{
    public class KeyBindingRebindUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI slotLabelText;
        [SerializeField] private TextMeshProUGUI currentBindingText;
        [SerializeField] private Button rebindButton;
        [SerializeField] private Button resetButton;
        [SerializeField] private GameObject waitingForInputPanel;
        [SerializeField] private TextMeshProUGUI waitingForInputText;
        
        [Header("Settings")]
        [SerializeField] private int slotIndex;
        [SerializeField] private KeyBindingSettingsManager settingsManager;
        
        private InputActionRebindingExtensions.RebindingOperation _rebindingOperation;
        private InputAction _currentInputAction;
        
        public int SlotIndex
        {
            get => slotIndex;
            set
            {
                slotIndex = value;
                UpdateUI();
            }
        }
        
        private void Start()
        {
            if (!settingsManager)
            {
                settingsManager = FindAnyObjectByType<KeyBindingSettingsManager>();
            }
            
            if (rebindButton)
            {
                rebindButton.onClick.AddListener(StartRebinding);
            }
            
            if (resetButton)
            {
                resetButton.onClick.AddListener(ResetToDefault);
            }
            
            if (settingsManager)
            {
                settingsManager.OnBindingChanged += OnBindingChanged;
            }
            
            UpdateUI();
        }
        
        private void OnDestroy()
        {
            if (_rebindingOperation != null)
            {
                _rebindingOperation.Cancel();
            }
            
            if (settingsManager)
            {
                settingsManager.OnBindingChanged -= OnBindingChanged;
            }
            
            if (rebindButton)
            {
                rebindButton.onClick.RemoveAllListeners();
            }
            
            if (resetButton)
            {
                resetButton.onClick.RemoveAllListeners();
            }
        }
        
        private void UpdateUI()
        {
            if (!settingsManager) return;
            
            var abilitySlotSystem = FindAnyObjectByType<AbilitySlotSystem>();
            if (abilitySlotSystem)
            {
                _currentInputAction = abilitySlotSystem.GetSlotInputAction(slotIndex);
                
                if (_currentInputAction != null)
                {
                    string displayString = abilitySlotSystem.GetSlotBindingDisplayString(slotIndex);
                    if (currentBindingText)
                    {
                        currentBindingText.text = FormatBindingDisplayString(displayString);
                    }
                }
                else
                {
                    _currentInputAction = settingsManager.GetInputActionForSlot(slotIndex);
                    if (_currentInputAction != null)
                    {
                        string displayString = settingsManager.GetSlotBindingDisplayString(slotIndex);
                        if (currentBindingText)
                        {
                            currentBindingText.text = FormatBindingDisplayString(displayString);
                        }
                    }
                    else
                    {
                        if (currentBindingText)
                        {
                            currentBindingText.text = "Не назначено";
                        }
                    }
                }
            }
            else
            {
                _currentInputAction = settingsManager.GetInputActionForSlot(slotIndex);
                if (_currentInputAction != null)
                {
                    string displayString = settingsManager.GetSlotBindingDisplayString(slotIndex);
                    if (currentBindingText)
                    {
                        currentBindingText.text = FormatBindingDisplayString(displayString);
                    }
                }
                else
                {
                    if (currentBindingText)
                    {
                        currentBindingText.text = "Не назначено";
                    }
                }
            }
        }
        
        private string FormatBindingDisplayString(string displayString)
        {
            if (string.IsNullOrEmpty(displayString)) return "Не назначено";
            
            displayString = displayString.Replace("|", "").Trim();
            
            if (_currentInputAction != null && (displayString == "Button" || displayString == "X" || string.IsNullOrEmpty(displayString)))
            {
                var bindings = _currentInputAction.bindings;
                for (int i = 0; i < bindings.Count; i++)
                {
                    var binding = bindings[i];
                    if (binding.isComposite) continue;
                    
                    string path = binding.effectivePath ?? binding.path;
                    if (!string.IsNullOrEmpty(path))
                    {
                        if (path.Contains("<Mouse>/leftButton") || path.Contains("<Mouse>/button0"))
                        {
                            return "ЛКМ";
                        }
                        else if (path.Contains("<Mouse>/rightButton") || path.Contains("<Mouse>/button1"))
                        {
                            return "ПКМ";
                        }
                        else if (path.Contains("<Mouse>/middleButton") || path.Contains("<Mouse>/button2"))
                        {
                            return "СКМ";
                        }
                        else if (path.Contains("<Keyboard>/"))
                        {
                            int startIndex = path.LastIndexOf('/') + 1;
                            if (startIndex > 0 && startIndex < path.Length)
                            {
                                string keyName = path.Substring(startIndex);
                                return FormatKeyName(keyName);
                            }
                        }
                    }
                }
            }
            
            if (displayString.Contains("Left Button") || displayString.Contains("leftButton") || 
                displayString.Contains("LeftButton") || displayString.ToLower().Contains("left button"))
            {
                return "ЛКМ";
            }
            else if (displayString.Contains("Right Button") || displayString.Contains("rightButton") ||
                     displayString.Contains("RightButton") || displayString.ToLower().Contains("right button"))
            {
                return "ПКМ";
            }
            else if (displayString.Contains("Middle Button") || displayString.Contains("middleButton") ||
                     displayString.Contains("MiddleButton") || displayString.ToLower().Contains("middle button"))
            {
                return "СКМ";
            }
            
            int bracketIndex = displayString.IndexOf('[');
            if (bracketIndex > 0)
            {
                string keyName = displayString.Substring(0, bracketIndex).Trim();
                keyName = keyName.Replace("|", "").Trim();
                return keyName;
            }
            
            displayString = displayString.Replace("|", "").Trim();
            
            string[] parts = displayString.Split(new char[] { ' ', '\t' }, System.StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length > 0)
            {
                return parts[0];
            }
            
            return displayString;
        }
        
        private string FormatKeyName(string keyName)
        {
            keyName = keyName.Replace("alpha", "").Replace("Alpha", "");
            keyName = keyName.Replace("keypad", "").Replace("Keypad", "");
            
            if (keyName.Equals("x", System.StringComparison.OrdinalIgnoreCase))
            {
                return "X";
            }
            
            return keyName;
        }
        
        public void StartRebinding()
        {
            if (!settingsManager) return;
            
            var abilitySlotSystem = FindAnyObjectByType<AbilitySlotSystem>();
            if (abilitySlotSystem)
            {
                _currentInputAction = abilitySlotSystem.GetSlotInputAction(slotIndex);
            }
            
            if (_currentInputAction == null)
            {
                _currentInputAction = settingsManager.GetInputActionForSlot(slotIndex);
            }
            
            if (_currentInputAction == null)
            {
                return;
            }
            
            if (waitingForInputPanel)
            {
                waitingForInputPanel.SetActive(true);
            }
            
            if (waitingForInputText)
            {
                waitingForInputText.text = "Нажмите клавишу для переназначения...\n(ESC для отмены)";
            }
            
            if (rebindButton)
            {
                rebindButton.interactable = false;
            }
            
            if (resetButton)
            {
                resetButton.interactable = false;
            }
            
            _currentInputAction.Disable();
            
            _rebindingOperation = _currentInputAction.PerformInteractiveRebinding()
                .WithCancelingThrough("<Keyboard>/escape")
                .OnMatchWaitForAnother(0.1f)
                .OnComplete(operation =>
                {
                    OnRebindComplete(operation);
                })
                .OnCancel(operation =>
                {
                    OnRebindCanceled(operation);
                })
                .Start();
        }
        
        private void OnRebindComplete(InputActionRebindingExtensions.RebindingOperation operation)
        {
            if (_currentInputAction != null)
            {
                _currentInputAction.Enable();
            }
            
            var inputActionAsset = settingsManager?.InputActionAsset;
            if (inputActionAsset)
            {
                var rebindOverrides = _currentInputAction.SaveBindingOverridesAsJson();
                PlayerPrefs.SetString($"InputRebind_{_currentInputAction.actionMap.name}_{_currentInputAction.name}", rebindOverrides);
                PlayerPrefs.Save();
            }
            
            var abilitySlotSystem = FindAnyObjectByType<AbilitySlotSystem>();
            if (abilitySlotSystem)
            {
                abilitySlotSystem.NotifyBindingChanged(slotIndex);
            }
            
            if (settingsManager)
            {
                OnBindingChanged(slotIndex, _currentInputAction.name);
            }
            
            operation.Dispose();
            _rebindingOperation = null;

            if (waitingForInputPanel)
            {
                waitingForInputPanel.SetActive(false);
            }

            if (rebindButton)
            {
                rebindButton.interactable = true;
            }
            
            if (resetButton)
            {
                resetButton.interactable = true;
            }

            UpdateUI();
        }
        
        private void OnRebindCanceled(InputActionRebindingExtensions.RebindingOperation operation)
        {
            if (_currentInputAction != null)
            {
                _currentInputAction.Enable();
            }
            
            operation.Dispose();
            _rebindingOperation = null;
            
            if (waitingForInputPanel)
            {
                waitingForInputPanel.SetActive(false);
            }

            if (rebindButton)
            {
                rebindButton.interactable = true;
            }
            
            if (resetButton)
            {
                resetButton.interactable = true;
            }
        }
        
        private void ResetToDefault()
        {
            if (!settingsManager) return;
            
            var abilitySlotSystem = FindAnyObjectByType<AbilitySlotSystem>();
            InputAction currentAction = null;
            
            if (abilitySlotSystem)
            {
                currentAction = abilitySlotSystem.GetSlotInputAction(slotIndex);
            }
            
            if (currentAction == null)
            {
                currentAction = settingsManager.GetInputActionForSlot(slotIndex);
            }
            
            if (currentAction == null) return;

            currentAction.RemoveAllBindingOverrides();
            
            var inputActionAsset = settingsManager.InputActionAsset;
            if (inputActionAsset)
            {
                PlayerPrefs.DeleteKey($"InputRebind_{currentAction.actionMap.name}_{currentAction.name}");
                PlayerPrefs.Save();
            }

            if (abilitySlotSystem)
            {
                abilitySlotSystem.NotifyBindingChanged(slotIndex);
            }
            
            settingsManager.ResetSlotToDefault(slotIndex);
            
            UpdateUI();
        }
        
        private void OnBindingChanged(int changedSlotIndex, string actionName)
        {
            if (changedSlotIndex == slotIndex)
            {
                UpdateUI();
            }
        }
    }
}

