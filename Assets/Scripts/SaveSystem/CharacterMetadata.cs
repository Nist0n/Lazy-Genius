using System;
using UnityEngine;

namespace SaveSystem
{
    [Serializable]
    public class CharacterMetadata
    {
        public string characterGuid;
        public string characterName;
        public string className;
        public int currentLevel;
        public long creationTimestamp;
        public long lastPlayedTimestamp;
        public float totalPlaytimeSeconds;
        
        [NonSerialized] public Sprite classIcon;
        
        public static CharacterMetadata FromSaveData(CharacterSaveData saveData)
        {
            return new CharacterMetadata
            {
                characterGuid = saveData.characterGuid,
                characterName = saveData.characterName,
                className = saveData.className,
                currentLevel = saveData.currentLevel,
                creationTimestamp = saveData.creationTimestamp,
                lastPlayedTimestamp = saveData.lastPlayedTimestamp,
                totalPlaytimeSeconds = saveData.totalPlaytimeSeconds
            };
        }
        
        public string GetCreationDate()
        {
            DateTime dateTime = DateTimeOffset.FromUnixTimeSeconds(creationTimestamp).LocalDateTime;
            return dateTime.ToString("dd.MM.yyyy");
        }
        
        public string GetLastPlayedDate()
        {
            DateTime dateTime = DateTimeOffset.FromUnixTimeSeconds(lastPlayedTimestamp).LocalDateTime;
            return dateTime.ToString("dd.MM.yyyy HH:mm");
        }
        
        public string GetFormattedPlaytime()
        {
            TimeSpan timeSpan = TimeSpan.FromSeconds(totalPlaytimeSeconds);
            
            if (timeSpan.TotalHours >= 1)
            {
                return $"{(int)timeSpan.TotalHours}ч {timeSpan.Minutes}м";
            }
            else if (timeSpan.TotalMinutes >= 1)
            {
                return $"{(int)timeSpan.TotalMinutes}м";
            }
            else
            {
                return "< 1м";
            }
        }
    }
}
