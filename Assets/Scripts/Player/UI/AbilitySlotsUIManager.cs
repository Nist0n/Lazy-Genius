using UnityEngine;
using Player;

namespace Player.UI
{
    public class AbilitySlotsUIManager : MonoBehaviour
    {
        [Header("Slot UI References")]
        [SerializeField] private AbilitySlotUI[] slotUIs = new AbilitySlotUI[5];
        
        private AbilitySlotSystem _abilitySlotSystem;
        
        private void Start()
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player)
            {
                _abilitySlotSystem = player.GetComponent<AbilitySlotSystem>();
                
                if (_abilitySlotSystem)
                {
                    for (int i = 0; i < slotUIs.Length && i < _abilitySlotSystem.MaxSlots; i++)
                    {
                        if (slotUIs[i])
                        {
                            slotUIs[i].SetSlotIndex(i);
                        }
                    }
                }
            }
        }
    }
}

