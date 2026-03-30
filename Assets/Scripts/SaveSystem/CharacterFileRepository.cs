using System;
using System.Collections.Generic;
using System.IO;
using Abstractions.Save;
using UnityEngine;

namespace SaveSystem
{
    public sealed class CharacterFileRepository : ICharacterRepository
    {
        public event Action IndexUpdated;

        private const string SAVE_FOLDER = "Saves";
        private const string CHARACTER_PREFIX = "character_";
        private const string CHARACTER_INDEX = "characters_index.json";
        private const string FILE_EXTENSION = ".json";

        private readonly string _savePath;
        private CharacterIndexData _characterIndex;

        [Serializable]
        private class CharacterIndexData
        {
            public List<CharacterMetadata> characters = new List<CharacterMetadata>();
        }

        public CharacterFileRepository()
        {
            _savePath = Path.Combine(Application.persistentDataPath, SAVE_FOLDER);

            if (!Directory.Exists(_savePath))
            {
                Directory.CreateDirectory(_savePath);
            }

            LoadCharacterIndex();
        }

        public bool SaveCharacter(CharacterSaveData saveData, CharacterMetadata metadata)
        {
            try
            {
                string json = JsonUtility.ToJson(saveData, true);

                string fileName = GetCharacterFileName(saveData.characterGuid);
                string filePath = Path.Combine(_savePath, fileName);
                File.WriteAllText(filePath, json);

                UpdateCharacterInIndex(metadata);
                SaveCharacterIndex();

                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[CharacterFileRepository] Failed to save character: {e.Message}");
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
                Debug.LogError($"[CharacterFileRepository] Failed to load character: {e.Message}");
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
                Debug.LogError($"[CharacterFileRepository] Failed to delete character: {e.Message}");
                return false;
            }
        }

        public List<CharacterMetadata> GetAllCharacters()
        {
            if (_characterIndex != null) return _characterIndex.characters;
            return new List<CharacterMetadata>();
        }

        private void LoadCharacterIndex()
        {
            try
            {
                string indexPath = Path.Combine(_savePath, CHARACTER_INDEX);

                if (File.Exists(indexPath))
                {
                    string json = File.ReadAllText(indexPath);
                    _characterIndex = JsonUtility.FromJson<CharacterIndexData>(json);

                    if (_characterIndex == null)
                    {
                        _characterIndex = new CharacterIndexData();
                    }
                }
                else
                {
                    _characterIndex = new CharacterIndexData();
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[CharacterFileRepository] Failed to load character index: {e.Message}");
                _characterIndex = new CharacterIndexData();
            }
        }

        private void SaveCharacterIndex()
        {
            try
            {
                string indexPath = Path.Combine(_savePath, CHARACTER_INDEX);
                string json = JsonUtility.ToJson(_characterIndex, true);
                File.WriteAllText(indexPath, json);

                IndexUpdated?.Invoke();
            }
            catch (Exception e)
            {
                Debug.LogError($"[CharacterFileRepository] Failed to save character index: {e.Message}");
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
}

