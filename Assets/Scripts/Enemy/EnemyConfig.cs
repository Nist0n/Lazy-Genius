using UnityEngine;

namespace Enemy
{
    [CreateAssetMenu(fileName = "NewEnemyConfig", menuName = "Lazy-Genius/Enemy/Enemy Config")]
    public class EnemyConfig : ScriptableObject
    {
        [Header("Base Stats")]
        [Tooltip("Максимальное здоровье")]
        public float MaxHealth = 100f;
        
        [Tooltip("Скорость передвижения")]
        public float MoveSpeed = 3f;

        [Header("Combat")]
        [Tooltip("Урон при атаке")]
        public float AttackDamage = 10f;
        
        [Tooltip("Дистанция атаки")]
        public float AttackRange = 4f;
        
        [Tooltip("Кулдаун атаки (сек)")]
        public float AttackCooldown = 1.0f;
        
        [Tooltip("Время стана после получения урона")]
        public float GetHitCooldown = 0.5f;
        
        [Tooltip("Радиус обнаружения игрока")]
        public float DetectionRadius = 10f;
        
        [Tooltip("Угол обзора (градусы)")]
        [Range(0f, 360f)]
        public float FieldOfView = 110f;
    }
}
