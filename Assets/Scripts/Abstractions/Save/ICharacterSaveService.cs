using System;
using System.Collections.Generic;
using SaveSystem;

namespace Abstractions.Save
{
    public interface ICharacterSaveService
    {
        public event Action IndexUpdated;
        bool SaveCharacter(CharacterData characterData);
        CharacterSaveData LoadCharacter(string characterGuid);
        bool DeleteCharacter(string characterGuid);
        List<CharacterMetadata> GetAllCharacters();
    }
}

