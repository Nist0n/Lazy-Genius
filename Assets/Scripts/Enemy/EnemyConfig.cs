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

        [Header("Ranged Enemy Settings")]
        [Tooltip("Является ли враг дальнего боя")]
        public bool IsRangedEnemy = false;

        [Tooltip("Минимальная желаемая дистанция до игрока для стрельбы")]
        public float PreferredMinDistance = 15f;

        [Tooltip("Максимальная желаемая дистанция до игрока для стрельбы")]
        public float PreferredMaxDistance = 25f;

        [Tooltip("Дистанция, при которой враг начинает активно убегать от игрока")]
        public float RunAwayDistance = 10f;

        [Tooltip("Префаб снаряда, которым стреляет враг дальнего боя")]
        public GameObject ProjectilePrefab;

        [Tooltip("Урон снаряда (если 0, берётся AttackDamage)")]
        public float ProjectileDamage = 0f;
    }
}
