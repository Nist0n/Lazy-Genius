using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace SaveSystem
{
    public class SaveManager : MonoBehaviour
    {
        private static SaveManager _instance;
        public static SaveManager Instance
        {
            get
            {
                if (!_instance)
                {
                    GameObject go = new GameObject("SaveManager");
                    _instance = go.AddComponent<SaveManager>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }
        
        private const string SAVE_FOLDER = "Saves";
        private const string CHARACTER_PREFIX = "character_";
        private const string CHARACTER_INDEX = "characters_index.json";
        private const string FILE_EXTENSION = ".json";
        
        private string _savePath;
        private CharacterIndex _characterIndex;

        public event Action OnIndexUpdated;
        
        private void Awake()
        {
            if (_instance && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            _instance = this;
            DontDestroyOnLoad(gameObject);
            
            InitializeSaveSystem();
        }
        
        private void InitializeSaveSystem()
        {
            _savePath = Path.Combine(Application.persistentDataPath, SAVE_FOLDER);
            
            if (!Directory.Exists(_savePath))
            {
                Directory.CreateDirectory(_savePath);
            }
            
            LoadCharacterIndex();
        }
        
        public bool SaveCharacter(CharacterData characterData)
        {
            try
            {
                characterData.LastPlayed = DateTime.UtcNow;
                characterData.UpdatePlaytime();
                
                CharacterSaveData saveData = characterData.ToSaveData();
                saveData.UpdateLastPlayed();

                string json = JsonUtility.ToJson(saveData, true);

                string fileName = GetCharacterFileName(characterData.CharacterGuid);
                string filePath = Path.Combine(_savePath, fileName);
                File.WriteAllText(filePath, json);

                UpdateCharacterInIndex(characterData.GetMetadata());
                SaveCharacterIndex();
                
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }
        
        public CharacterSaveData LoadCharacter(string characterGuid)
        {
            try
            {
                string fileName = GetCharacterFileName(characterGuid);
                string filePath = Path.Combine(_savePath, fileName);
                
                if (!File.Exists(filePath))
                {
                    return null;
                }

                string json = File.ReadAllText(filePath);

                CharacterSaveData saveData = JsonUtility.FromJson<CharacterSaveData>(json);
                
                if (saveData == null)
                {
                    return null;
                }
                
                return saveData;
            }
            catch (Exception e)
            {
                return null;
            }
        }
        
        public bool DeleteCharacter(string characterGuid)
        {
            try
            {
                string fileName = GetCharacterFileName(characterGuid);
                string filePath = Path.Combine(_savePath, fileName);
                
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }

                RemoveCharacterFromIndex(characterGuid);
                SaveCharacterIndex();
                
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }
        
        public List<CharacterMetadata> GetAllCharacters()
        {
            if (_characterIndex != null) return _characterIndex.characters;
            else return new List<CharacterMetadata>();
        }
        
        private void LoadCharacterIndex()
        {
            try
            {
                string indexPath = Path.Combine(_savePath, CHARACTER_INDEX);
                
                if (File.Exists(indexPath))
                {
                    string json = File.ReadAllText(indexPath);
                    _characterIndex = JsonUtility.FromJson<CharacterIndex>(json);
                    
                    if (_characterIndex == null)
                    {
                        _characterIndex = new CharacterIndex();
                    }
                }
                else
                {
                    _characterIndex = new CharacterIndex();
                }
            }
            catch (Exception e)
            {
                _characterIndex = new CharacterIndex();
            }
        }
        
        private void SaveCharacterIndex()
        {
            try
            {
                string indexPath = Path.Combine(_savePath, CHARACTER_INDEX);
                string json = JsonUtility.ToJson(_characterIndex, true);
                File.WriteAllText(indexPath, json);
                
                OnIndexUpdated?.Invoke();
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveManager] Failed to save character index: {e.Message}");
            }
        }
        
        private void UpdateCharacterInIndex(CharacterMetadata metadata)
        {
            int existingIndex = _characterIndex.characters.FindIndex(c => c.characterGuid == metadata.characterGuid);
            
            if (existingIndex >= 0)
            {
                _characterIndex.characters[existingIndex] = metadata;
            }
            else
            {
                _characterIndex.characters.Add(metadata);
            }
        }
        
        private void RemoveCharacterFromIndex(string characterGuid)
        {
            _characterIndex.characters.RemoveAll(c => c.characterGuid == characterGuid);
        }
        
        private string GetCharacterFileName(string characterGuid)
        {
            return CHARACTER_PREFIX + characterGuid + FILE_EXTENSION;
        }
    }
    
    [Serializable]
    public class CharacterIndex
    {
        public List<CharacterMetadata> characters = new List<CharacterMetadata>();
    }
}
