using UnityEngine;
using SaveSystem;

namespace Player
{
    [RequireComponent(typeof(PlayerController))]
    public class PlayerCharacterLoader : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private bool loadOnStart = true;
        [SerializeField] private bool saveOnDestroy = true;
        [SerializeField] private float autoSaveInterval = 300f;
        
        private PlayerController _playerController;
        private float _timeSinceLastSave;
        
        private void Awake()
        {
            _playerController = GetComponent<PlayerController>();
        }
        
        private void Start()
        {
            if (loadOnStart)
            {
                LoadCharacterData();
            }
        }
        
        private void Update()
        {
            // Auto-save
            if (autoSaveInterval > 0)
            {
                _timeSinceLastSave += Time.deltaTime;
                
                if (_timeSinceLastSave >= autoSaveInterval)
                {
                    SaveCharacterData();
                    _timeSinceLastSave = 0f;
                }
            }
        }
        
        private void OnDestroy()
        {
            if (saveOnDestroy)
            {
                SaveCharacterData();
            }
        }

        public void LoadCharacterData()
        {
            if (!CharacterManager.Instance)
            {
                return;
            }
            
            if (!CharacterManager.Instance.HasActiveCharacter)
            {
                return;
            }
            
            CharacterData activeCharacter = CharacterManager.Instance.ActiveCharacter;
            
            if (_playerController)
            {
                _playerController.LoadFromCharacterData(activeCharacter);
            }
        }
        
        public void SaveCharacterData()
        {
            if (!CharacterManager.Instance || !CharacterManager.Instance.HasActiveCharacter)
            {
                return;
            }
            
            CharacterData activeCharacter = CharacterManager.Instance.ActiveCharacter;
            
            if (_playerController)
            {
                _playerController.SaveToCharacterData(activeCharacter);
                CharacterManager.Instance.SaveActiveCharacter();
            }
        }
    }
}
