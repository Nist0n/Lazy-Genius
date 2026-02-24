using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.Input
{
    public class InputOverrideLoader : MonoBehaviour
    {
        [SerializeField] private InputActionAsset inputActionAsset;
        [SerializeField] private bool loadOnAwake = true;

        private void Awake()
        {
            if (loadOnAwake)
            {
                LoadOverrides();
            }
        }

        private void Start()
        {
            if (!inputActionAsset)
            {
                 var playerInput = FindAnyObjectByType<PlayerInput>();
                 if (playerInput) inputActionAsset = playerInput.actions;
                 
                 LoadOverrides();
            }
        }

        public void LoadOverrides()
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
                    }
                }
            }
        }
    }
}
