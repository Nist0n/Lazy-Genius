using System;
using System.Collections.Generic;
using Player;
using UnityEngine;

namespace SaveSystem
{
    public class CharacterData
    {
        // Metadata
        public string CharacterGuid { get; private set; }
        public string CharacterName { get; set; }
        public PlayerClass PlayerClass { get; set; }
        public DateTime CreationDate { get; private set; }
        public DateTime LastPlayed { get; set; }
        public float TotalPlaytime { get; set; }
        
        // Progression
        public int CurrentLevel { get; set; }
        public int CurrentExperience { get; set; }
        public int ExperienceToNextLevel { get; set; }
        
        // Current State
        public float CurrentHealth { get; set; }
        public float MaxHealth { get; set; }
        public float CurrentEnergy { get; set; }
        public float MaxEnergy { get; set; }
        
        // Playtime tracking
        private float _sessionStartTime;
        
        public CharacterData(string name, PlayerClass playerClass)
        {
            CharacterGuid = Guid.NewGuid().ToString();
            CharacterName = name;
            PlayerClass = playerClass;
            CreationDate = DateTime.UtcNow;
            LastPlayed = DateTime.UtcNow;
            TotalPlaytime = 0f;
            
            CurrentLevel = 1;
            CurrentExperience = 0;
            ExperienceToNextLevel = 100;
        }
        
        public CharacterData(CharacterSaveData saveData, PlayerClass playerClass)
        {
            CharacterGuid = saveData.characterGuid;
            CharacterName = saveData.characterName;
            PlayerClass = playerClass;
            CreationDate = DateTimeOffset.FromUnixTimeSeconds(saveData.creationTimestamp).UtcDateTime;
            LastPlayed = DateTimeOffset.FromUnixTimeSeconds(saveData.lastPlayedTimestamp).UtcDateTime;
            TotalPlaytime = saveData.totalPlaytimeSeconds;
            
            CurrentLevel = saveData.currentLevel;
            CurrentExperience = saveData.currentExperience;
            ExperienceToNextLevel = saveData.experienceToNextLevel;
            
            CurrentHealth = saveData.currentHealth;
            MaxHealth = saveData.maxHealth;
            CurrentEnergy = saveData.currentEnergy;
            MaxEnergy = saveData.maxEnergy;
        }
        
        public CharacterSaveData ToSaveData()
        {
            CharacterSaveData saveData = new CharacterSaveData
            {
                characterGuid = CharacterGuid,
                characterName = CharacterName,
                className = PlayerClass ? PlayerClass.ClassName : "",
                creationTimestamp = new DateTimeOffset(CreationDate).ToUnixTimeSeconds(),
                lastPlayedTimestamp = new DateTimeOffset(LastPlayed).ToUnixTimeSeconds(),
                totalPlaytimeSeconds = TotalPlaytime,
                
                currentLevel = CurrentLevel,
                currentExperience = CurrentExperience,
                experienceToNextLevel = ExperienceToNextLevel,
                
                currentHealth = CurrentHealth,
                maxHealth = MaxHealth,
                currentEnergy = CurrentEnergy,
                maxEnergy = MaxEnergy,
            };
            
            return saveData;
        }
        
        public void StartPlaytimeTracking()
        {
            _sessionStartTime = Time.realtimeSinceStartup;
        }

        public void UpdatePlaytime()
        {
            if (_sessionStartTime > 0)
            {
                float sessionTime = Time.realtimeSinceStartup - _sessionStartTime;
                TotalPlaytime += sessionTime;
                _sessionStartTime = Time.realtimeSinceStartup;
            }
        }
        
        public CharacterMetadata GetMetadata()
        {
            return new CharacterMetadata
            {
                characterGuid = CharacterGuid,
                characterName = CharacterName,
                className = PlayerClass ? PlayerClass.ClassName : "Unknown",
                currentLevel = CurrentLevel,
                creationTimestamp = new DateTimeOffset(CreationDate).ToUnixTimeSeconds(),
                lastPlayedTimestamp = new DateTimeOffset(LastPlayed).ToUnixTimeSeconds(),
                totalPlaytimeSeconds = TotalPlaytime,
                classIcon = PlayerClass ? PlayerClass.ClassIcon : null
            };
        }
    }
}
