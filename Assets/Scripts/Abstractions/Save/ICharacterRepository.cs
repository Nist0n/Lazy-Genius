using System;
using System.Collections.Generic;
using SaveSystem;

namespace Abstractions.Save
{
    public interface ICharacterRepository
    {
        event Action IndexUpdated;

        bool SaveCharacter(CharacterSaveData saveData, CharacterMetadata metadata);
        CharacterSaveData LoadCharacter(string characterGuid);
        bool DeleteCharacter(string characterGuid);
        List<CharacterMetadata> GetAllCharacters();
    }
}

