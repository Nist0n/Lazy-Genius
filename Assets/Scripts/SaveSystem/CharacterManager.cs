using System;
using System.Collections.Generic;
using Player;
using UnityEngine;

namespace SaveSystem
{
    public class CharacterManager : MonoBehaviour
    {
        private static CharacterManager _instance;
        public static CharacterManager Instance
        {
            get
            {
                if (!_instance)
                {
                    GameObject go = new GameObject("CharacterManager");
                    _instance = go.AddComponent<CharacterManager>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }
        
        [Header("Available Classes")]
        [SerializeField] private List<PlayerClass> availableClasses = new List<PlayerClass>();
        
        private CharacterData _activeCharacter;
        private List<CharacterMetadata> _cachedCharacterList;
        
        public event Action<CharacterData> OnCharacterCreated;
        public event Action<string> OnCharacterDeleted;
        
        public CharacterData ActiveCharacter => _activeCharacter;
        public bool HasActiveCharacter => _activeCharacter != null;
        
        private void Awake()
        {
            if (_instance && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            _instance = this;
            DontDestroyOnLoad(gameObject);

            if (availableClasses == null || availableClasses.Count == 0)
            {
                LoadAvailableClasses();
            }
            
            SaveManager.Instance.OnIndexUpdated += RefreshCharacterList;
            
            RefreshCharacterList();
        }
        
        private void OnDestroy()
        {
            if (_instance == this)
            {
                SaveManager.Instance.OnIndexUpdated -= RefreshCharacterList;
            }
        }
        
        public CharacterData CreateCharacter(string characterName, PlayerClass playerClass)
        {
            if (string.IsNullOrWhiteSpace(characterName))
            {
                return null;
            }
            
            if (!playerClass)
            {
                return null;
            }
            
            if (IsCharacterNameTaken(characterName))
            {
                return null;
            }
            
            CharacterData newCharacter = new CharacterData(characterName, playerClass);
            
            InitializeNewCharacter(newCharacter, playerClass);
            
            if (SaveManager.Instance.SaveCharacter(newCharacter))
            {
                OnCharacterCreated?.Invoke(newCharacter);
                RefreshCharacterList();
                return newCharacter;
            }
            else
            {
                return null;
            }
        }
        
        public bool SelectCharacter(string characterGuid)
        {
            if (string.IsNullOrEmpty(characterGuid))
            {
                return false;
            }
            
            CharacterSaveData saveData = SaveManager.Instance.LoadCharacter(characterGuid);
            if (saveData == null)
            {
                return false;
            }
            
            PlayerClass playerClass = FindPlayerClassByName(saveData.className);
            if (!playerClass)
            {
                return false;
            }
            
            _activeCharacter = new CharacterData(saveData, playerClass);
            _activeCharacter.StartPlaytimeTracking();
            
            return true;
        }
        
        public bool DeleteCharacter(string characterGuid)
        {
            if (string.IsNullOrEmpty(characterGuid))
            {
                return false;
            }
            
            if (_activeCharacter != null && _activeCharacter.CharacterGuid == characterGuid)
            {
                return false;
            }
            
            if (SaveManager.Instance.DeleteCharacter(characterGuid))
            {
                OnCharacterDeleted?.Invoke(characterGuid);
                RefreshCharacterList();
                return true;
            }
            
            return false;
        }
        
        public bool SaveActiveCharacter()
        {
            if (_activeCharacter == null)
            {
                return false;
            }
            
            return SaveManager.Instance.SaveCharacter(_activeCharacter);
        }
        
        public void DeselectCharacter()
        {
            if (_activeCharacter != null)
            {
                SaveActiveCharacter();
                _activeCharacter = null;
            }
        }
        
        public List<CharacterMetadata> GetCharacterList()
        {
            var list = _cachedCharacterList;
            if (list != null)
            {
                return list;
            }

            return new List<CharacterMetadata>();
        }
        
        public List<PlayerClass> GetAvailableClasses()
        {
            return new List<PlayerClass>(availableClasses);
        }
        
        public bool IsCharacterNameTaken(string name)
        {
            if (_cachedCharacterList == null) return false;
            
            return _cachedCharacterList.Exists(c => 
                c.characterName.Equals(name, StringComparison.OrdinalIgnoreCase));
        }
        
        private void RefreshCharacterList()
        {
            _cachedCharacterList = SaveManager.Instance.GetAllCharacters();
            
            foreach (var metadata in _cachedCharacterList)
            {
                PlayerClass playerClass = FindPlayerClassByName(metadata.className);
                if (playerClass)
                {
                    metadata.classIcon = playerClass.ClassIcon;
                }
            }
        }
        
        private void InitializeNewCharacter(CharacterData character, PlayerClass playerClass)
        {
            character.CurrentHealth = 100f;
            character.MaxHealth = 100f;
            character.CurrentEnergy = 100f;
            character.MaxEnergy = 100f;
        }
        
        private void LoadAvailableClasses()
        {
            availableClasses = new List<PlayerClass>();
            
            PlayerClass[] allClasses = Resources.LoadAll<PlayerClass>("ScriptableObjects/Player/Classes");
            
            if (allClasses.Length > 0)
            {
                availableClasses.AddRange(allClasses);
            }
        }
        
        private PlayerClass FindPlayerClassByName(string className)
        {
            PlayerClass found = availableClasses.Find(c => c.ClassName == className);
            if (found) return found;
            
            PlayerClass[] allClasses = Resources.LoadAll<PlayerClass>("ScriptableObjects/Player/Classes");
            foreach (var playerClass in allClasses)
            {
                if (playerClass.ClassName == className)
                {
                    if (!availableClasses.Contains(playerClass))
                    {
                        availableClasses.Add(playerClass);
                    }
                    return playerClass;
                }
            }
            
            return null;
        }
        
        private void Update()
        {
            // Auto-save
            if (_activeCharacter != null && Time.frameCount % (60 * 60 * 5) == 0)
            {
                SaveActiveCharacter();
            }
        }
    }
}
