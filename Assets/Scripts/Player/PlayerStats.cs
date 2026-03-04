using UnityEngine;

namespace Player
{
    [CreateAssetMenu(fileName = "PlayerStats", menuName = "Lazy-Genius/Player/PlayerStats")]
    public class PlayerStats : ScriptableObject
    {
        [Header("Movement")]
        [Tooltip("Базовая скорость движения")]
        public float baseMoveSpeed = 5f;
        
        [Tooltip("Сила прыжка")]
        public float jumpForce = 5f;
        
        [Header("Health & Energy")]
        [Tooltip("Максимальное здоровье")]
        public float maxHealth = 100f;
        
        [Tooltip("Максимальная энергия")]
        public float maxEnergy = 100f;
    }
}

