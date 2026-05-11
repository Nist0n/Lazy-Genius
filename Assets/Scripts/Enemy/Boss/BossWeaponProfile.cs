using UnityEngine;

namespace Enemy.Boss
{
    [CreateAssetMenu(fileName = "BossWeaponProfile", menuName = "Lazy-Genius/Boss/Weapon Profile")]
    public class BossWeaponProfile : ScriptableObject
    {
        [Header("Combat modifiers")]
        [SerializeField] private float damageMultiplier = 1f;
        [SerializeField] private float attackSpeedMultiplier = 1f;

        [Header("Element visuals on bullets")]
        [SerializeField] private BossElementPresentationMode elementPresentation = BossElementPresentationMode.TintProjectileColor;

        [Header("Animator state names")]
        [SerializeField] private string basicAttackAnimState = "Attack";
        [SerializeField] private string suppressiveAttackAnimState = "Attack";
        [SerializeField] private string chaseAnimState = "Chase";
        
        public float DamageMultiplier => Mathf.Max(0f, damageMultiplier);
        public float AttackSpeedMultiplier => Mathf.Max(0.0001f, attackSpeedMultiplier);
        public BossElementPresentationMode ElementPresentation => elementPresentation;
        public string BasicAttackAnimState => string.IsNullOrWhiteSpace(basicAttackAnimState) ? "Attack" : basicAttackAnimState;
        public string SuppressiveAttackAnimState => string.IsNullOrWhiteSpace(suppressiveAttackAnimState) ? "Attack" : suppressiveAttackAnimState;
        public string ChaseAnimState => string.IsNullOrWhiteSpace(chaseAnimState) ? "Chase" : chaseAnimState;
    }
}
