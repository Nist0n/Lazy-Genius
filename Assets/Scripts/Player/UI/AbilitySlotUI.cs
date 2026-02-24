using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Player;

namespace Player.UI
{
    public class AbilitySlotUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Image iconImage;
        [SerializeField] private Image cooldownOverlay;
        [SerializeField] private TextMeshProUGUI cooldownText;
        [SerializeField] private TextMeshProUGUI keyBindingText;
        [SerializeField] private TextMeshProUGUI energyCostText;
        [SerializeField] private GameObject activeIndicator;
        
        [Header("Slot Info")]
        [SerializeField] private int slotIndex;
        
        private AbilitySlotSystem _abilitySlotSystem;
        private AbilitySystem _abilitySystem;
        private float _lastCooldownValue = -1f;
        
        public int SlotIndex => slotIndex;
        
        public void SetSlotIndex(int index)
        {
            slotIndex = index;
        }
        
        private void Start()
        {
            Invoke(nameof(DelayedInitialize), 0.1f);
        }
        
        private void DelayedInitialize()
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player)
            {
                _abilitySlotSystem = player.GetComponent<AbilitySlotSystem>();
                _abilitySystem = player.GetComponent<AbilitySystem>();
                
                if (_abilitySlotSystem)
                {
                    _abilitySlotSystem.OnAbilityAssignedToSlot += OnAbilityAssigned;
                    _abilitySlotSystem.OnAbilityRemovedFromSlot += OnAbilityRemoved;
                    _abilitySlotSystem.OnSlotInputActionChanged += OnSlotInputActionChanged;
                    
                    UpdateKeyBinding();
                    
                    var ability = _abilitySlotSystem.GetAbilityFromSlot(slotIndex);
                    if (ability)
                    {
                        OnAbilityAssigned(slotIndex, ability);
                    }
                    else
                    {
                        ClearSlot();
                    }
                }
            }
        }
        
        private void Update()
        {
            UpdateCooldown();
            UpdateActiveState();
            if (Time.frameCount % 60 == 0)
            {
                UpdateKeyBinding();
            }
        }
        
        private void OnAbilityAssigned(int index, Ability ability)
        {
            if (index != slotIndex) return;
            
            if (ability)
            {
                if (iconImage && ability.icon)
                {
                    iconImage.sprite = ability.icon;
                    iconImage.enabled = true;
                }
                
                if (energyCostText)
                {
                    if (ability.energyCost > 0)
                    {
                        energyCostText.text = Mathf.RoundToInt(ability.energyCost).ToString();
                        energyCostText.gameObject.SetActive(true);
                    }
                    else
                    {
                        energyCostText.gameObject.SetActive(false);
                    }
                }
                
                UpdateKeyBinding();
            }
            else
            {
                ClearSlot();
            }
        }
        
        private void OnAbilityRemoved(int index)
        {
            if (index != slotIndex) return;
            ClearSlot();
        }
        
        private void ClearSlot()
        {
            if (iconImage)
            {
                iconImage.sprite = null;
                iconImage.enabled = false;
            }
            
            if (cooldownOverlay)
            {
                cooldownOverlay.fillAmount = 0f;
            }
            
            if (cooldownText)
            {
                cooldownText.text = "";
            }
            
            if (energyCostText)
            {
                energyCostText.text = "";
                energyCostText.gameObject.SetActive(false);
            }
            
            if (activeIndicator)
            {
                activeIndicator.SetActive(false);
            }
        }
        
        private void UpdateCooldown()
        {
            if (!_abilitySystem) return;
            
            var ability = _abilitySlotSystem?.GetAbilityFromSlot(slotIndex);
            if (!ability) return;
            
            float cooldownRemaining = _abilitySystem.GetCooldownRemaining(slotIndex);
            float cooldownPercentage = _abilitySystem.GetCooldownPercentage(slotIndex);
            
            if (cooldownOverlay)
            {
                cooldownOverlay.fillAmount = cooldownPercentage;
            }
            
            if (cooldownText)
            {
                if (cooldownRemaining > 0f)
                {
                    float rounded = Mathf.Round(cooldownRemaining * 10f) / 10f;
                    if (!Mathf.Approximately(rounded, _lastCooldownValue))
                    {
                        _lastCooldownValue = rounded;
                        cooldownText.text = rounded.ToString("F1");
                    }
                }
                else
                {
                    _lastCooldownValue = -1f;
                    cooldownText.text = "";
                }
            }
        }
        
        private void UpdateActiveState()
        {
            if (!_abilitySystem || !activeIndicator) return;
            
            bool isActive = _abilitySystem.IsChanneledAbilityActive(slotIndex);
            activeIndicator.SetActive(isActive);
        }
        
        private void UpdateKeyBinding()
        {
            if (!_abilitySlotSystem || !keyBindingText) return;
            
            var ability = _abilitySlotSystem.GetAbilityFromSlot(slotIndex);
            if (!ability)
            {
                keyBindingText.text = "";
                return;
            }
            
            string bindingDisplayString = _abilitySlotSystem.GetSlotBindingDisplayString(slotIndex);
            if (!string.IsNullOrEmpty(bindingDisplayString))
            {
                keyBindingText.text = FormatBindingDisplayString(bindingDisplayString);
            }
            else
            {
                keyBindingText.text = "";
            }
        }
        
        private void OnSlotInputActionChanged(int index)
        {
            if (index == slotIndex)
            {
                UpdateKeyBinding();
            }
        }
        
        private string FormatBindingDisplayString(string displayString)
        {
            if (string.IsNullOrEmpty(displayString)) return "";
            
            displayString = displayString.Replace("|", "").Trim();
            
            if (displayString == "Button" || displayString == "X" || string.IsNullOrEmpty(displayString))
            {
                var inputAction = _abilitySlotSystem?.GetSlotInputAction(slotIndex);
                if (inputAction != null)
                {
                    var bindings = inputAction.bindings;
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
        
        private void OnDestroy()
        {
            if (_abilitySlotSystem)
            {
                _abilitySlotSystem.OnAbilityAssignedToSlot -= OnAbilityAssigned;
                _abilitySlotSystem.OnAbilityRemovedFromSlot -= OnAbilityRemoved;
                _abilitySlotSystem.OnSlotInputActionChanged -= OnSlotInputActionChanged;
            }
        }
    }
}

