using System;
using System.Collections.Generic;
using UnityEngine;

namespace SaveSystem
{
    [Serializable]
    public class CharacterSaveData
    {
        [Header("Character Metadata")]
        public string characterGuid;
        public string characterName;
        public string className;
        public long creationTimestamp;
        public long lastPlayedTimestamp;
        public float totalPlaytimeSeconds;
        
        [Header("Progression")]
        public int currentLevel;
        public int currentExperience;
        public int experienceToNextLevel;
        
        [Header("Health & Energy")]
        public float currentHealth;
        public float maxHealth;
        public float currentEnergy;
        public float maxEnergy;
        
        public CharacterSaveData()
        {
            characterGuid = Guid.NewGuid().ToString();
            creationTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            lastPlayedTimestamp = creationTimestamp;
            totalPlaytimeSeconds = 0f;
            
            currentLevel = 1;
            currentExperience = 0;
            experienceToNextLevel = 100;
        }
        
        public void UpdateLastPlayed()
        {
            lastPlayedTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        }
        
        public void AddPlaytime(float seconds)
        {
            totalPlaytimeSeconds += seconds;
        }
    }
}
