using UnityEngine;

namespace Enemy.Boss
{
    [CreateAssetMenu(fileName = "NewBossConfig", menuName = "Lazy-Genius/Enemy/Boss Config")]
    public class BossConfig : ScriptableObject
    {
        [Header("Base Stats")]
        public float MaxHealth = 600f;
        public float MoveSpeed = 4f;
        public float DetectionRadius = 50f;
        [Range(0f, 360f)] public float FieldOfView = 140f;

        [Header("Distance Windows")]
        public float ChaseMinDistance = 15f;
        public float CombatMinDistance = 8f;
        public float CombatMaxDistance = 15f;

        [Header("Basic Ranged Attack")]
        public GameObject BasicProjectilePrefab;
        public float BasicAttackDamage = 8f;
        public float BasicAimDelay = 0.5f;
        public Vector2Int BasicShotCountRange = new Vector2Int(2, 3);
        public float BasicInterval = 0.45f;

        [Header("Suppressive Fire")]
        public GameObject SuppressiveProjectilePrefab;
        public float SuppressiveDamage = 4f;
        public float SuppressiveDuration = 2f;
        public float SuppressiveShotsPerSecond = 8f;
        public float SuppressiveSpreadAngle = 9f;

        [Header("Rocket Barrage")]
        public GameObject RocketProjectilePrefab;
        public float RocketDamage = 16f;
        public Vector2Int RocketCountRange = new Vector2Int(4, 6);
        public float RocketCooldown = 15f;
        public float RocketTelegraphDuration = 1f;
        public float RocketAreaRadius = 5f;
        public float RocketExplosionRadius = 2.5f;
        public float RocketSpeed = 12f;
        public float RocketLifetime = 4f;
        public GameObject RocketTelegraphPrefab;

        [Header("Overheat")]
        public Vector2 OverheatDurationRange = new Vector2(3f, 4f);
        public float EnragedOverheatDuration = 2f;

        [Header("Enraged Modifier")]
        public float EnragedThresholdNormalized = 0.5f;
        public float EnragedMoveSpeedMultiplier = 1.4f;
        public float EnragedBasicAimDelay = 0.2f;
        public float EnragedSuppressiveBonusDuration = 1f;
        public int EnragedExtraRockets = 2;
        public float EnragedStateDuration = 0.8f;

        [Header("Death")]
        public float SelfDestructDelay = 5f;
    }
}
