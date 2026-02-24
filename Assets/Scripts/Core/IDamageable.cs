using UnityEngine;

namespace Core
{
    public interface IDamageable
    {
        void TakeDamage(float damage, DamageInfo damageInfo);
        float GetHealth();
        float GetMaxHealth();
        bool IsDead { get; }
    }
}
