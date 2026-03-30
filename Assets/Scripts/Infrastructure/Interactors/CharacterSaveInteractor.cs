using System;
using System.Collections.Generic;
using Abstractions.Save;
using Player;
using SaveSystem;
using UnityEngine;

namespace Infrastructure.Interactors
{
    public sealed class CharacterSaveInteractor : ICharacterSaveService
    {
        public event Action IndexUpdated;

        private readonly ICharacterRepository _repository;

        public CharacterSaveInteractor()
        {
            _repository = new CharacterFileRepository();
            _repository.IndexUpdated += OnRepositoryIndexUpdated;
        }

        public bool SaveCharacter(CharacterData characterData)
        {
            try
            {
                characterData.LastPlayed = DateTime.UtcNow;
                characterData.UpdatePlaytime();

                CharacterSaveData saveData = characterData.ToSaveData();
                saveData.UpdateLastPlayed();

                CharacterMetadata metadata = characterData.GetMetadata();

                return _repository.SaveCharacter(saveData, metadata);
            }
            catch (Exception e)
            {
                Debug.LogError($"[CharacterSaveInteractor] Failed to save character: {e.Message}");
                return false;
            }
        }

        public CharacterSaveData LoadCharacter(string characterGuid)
        {
            return _repository.LoadCharacter(characterGuid);
        }

        public bool DeleteCharacter(string characterGuid)
        {
            return _repository.DeleteCharacter(characterGuid);
        }

        public List<CharacterMetadata> GetAllCharacters()
        {
            return _repository.GetAllCharacters();
        }

        private void OnRepositoryIndexUpdated() => IndexUpdated?.Invoke();
    }
}

