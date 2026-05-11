using UnityEngine;

namespace Enemy.Boss
{
    [CreateAssetMenu(fileName = "BossElementProfile", menuName = "Lazy-Genius/Boss/Element Profile")]
    public class BossElementProfile : ScriptableObject
    {
        [SerializeField] private Color elementColor = Color.white;

        [Header("Projectiles (per element)")]
        [SerializeField] private GameObject basicProjectilePrefab;
        [SerializeField] private GameObject suppressiveProjectilePrefab;

        [Header("Minigun presentation")]
        [SerializeField] private GameObject projectileFollowParticlePrefab;

        public Color ElementColor => elementColor;
        public GameObject BasicProjectilePrefab => basicProjectilePrefab;
        public GameObject SuppressiveProjectilePrefab => suppressiveProjectilePrefab;
        public GameObject ProjectileFollowParticlePrefab => projectileFollowParticlePrefab;
    }
}
