using System;
using System.Collections.Generic;
using Abstractions.Characters;
using Player;
using SaveSystem;

namespace Infrastructure.Services
{
    public sealed class CharacterManagerCharacterService : ICharacterService
    {
        public event Action<CharacterData> CharacterCreated;
        public event Action<string> CharacterDeleted;

        public CharacterManagerCharacterService()
        {
            CharacterManager.Instance.OnCharacterCreated += OnCreated;
            CharacterManager.Instance.OnCharacterDeleted += OnDeleted;
        }

        public CharacterData CreateCharacter(string characterName, PlayerClass playerClass) =>
            CharacterManager.Instance.CreateCharacter(characterName, playerClass);

        public bool SelectCharacter(string characterGuid) =>
            CharacterManager.Instance.SelectCharacter(characterGuid);

        public bool DeleteCharacter(string characterGuid) =>
            CharacterManager.Instance.DeleteCharacter(characterGuid);

        public bool SaveActiveCharacter() =>
            CharacterManager.Instance.SaveActiveCharacter();

        public void DeselectCharacter() =>
            CharacterManager.Instance.DeselectCharacter();

        public List<CharacterMetadata> GetCharacterList() =>
            CharacterManager.Instance.GetCharacterList();

        public List<PlayerClass> GetAvailableClasses() =>
            CharacterManager.Instance.GetAvailableClasses();

        public bool IsCharacterNameTaken(string name) =>
            CharacterManager.Instance.IsCharacterNameTaken(name);

        private void OnCreated(CharacterData data) => CharacterCreated?.Invoke(data);
        private void OnDeleted(string guid) => CharacterDeleted?.Invoke(guid);
    }
}

