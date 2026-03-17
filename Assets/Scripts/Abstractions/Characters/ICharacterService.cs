using System;
using System.Collections.Generic;
using Player;
using SaveSystem;

namespace Abstractions.Characters
{
    public interface ICharacterService
    {
        event Action<CharacterData> CharacterCreated;
        event Action<string> CharacterDeleted;

        CharacterData ActiveCharacter { get; }
        bool HasActiveCharacter { get; }

        CharacterData CreateCharacter(string characterName, PlayerClass playerClass);
        bool SelectCharacter(string characterGuid);
        bool DeleteCharacter(string characterGuid);
        bool SaveActiveCharacter();
        void DeselectCharacter();

        List<CharacterMetadata> GetCharacterList();
        List<PlayerClass> GetAvailableClasses();
        bool IsCharacterNameTaken(string name);
    }
}

