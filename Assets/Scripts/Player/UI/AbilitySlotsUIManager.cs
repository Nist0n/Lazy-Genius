using UnityEngine;
using Player;

namespace Player.UI
{
    public class AbilitySlotsUIManager : MonoBehaviour
    {
        [Header("Slot UI References")]
        [SerializeField] private AbilitySlotUI[] slotUIs = new AbilitySlotUI[5];
        
        private AbilitySlotSystem _abilitySlotSystem;

        public void Initialize(AbilitySlotSystem abilitySlotSystem)
        {
            if (!abilitySlotSystem)
            {
                return;
            }

            _abilitySlotSystem = abilitySlotSystem;

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

