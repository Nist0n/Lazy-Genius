using System.Collections.Generic;
using Abstractions.Save;
using SaveSystem;

namespace Infrastructure.Services
{
    public sealed class SaveManagerCharacterSaveService : ICharacterSaveService
    {
        public bool SaveCharacter(CharacterData characterData) => SaveManager.Instance.SaveCharacter(characterData);

        public CharacterSaveData LoadCharacter(string characterGuid) => SaveManager.Instance.LoadCharacter(characterGuid);

        public bool DeleteCharacter(string characterGuid) => SaveManager.Instance.DeleteCharacter(characterGuid);

        public List<CharacterMetadata> GetAllCharacters() => SaveManager.Instance.GetAllCharacters();
    }
}

