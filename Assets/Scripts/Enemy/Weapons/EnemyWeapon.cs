using Abstractions.Weapons;
using UnityEngine;

namespace Enemy.Weapons
{
    public abstract class EnemyWeapon : Weapon
    {
        [Header("Enemy Animation Overrides")]
        [SerializeField] private string attackAnimState = "Attack";
        [SerializeField] private string chaseAnimState = "Chase";

        public virtual void ApplyTo(EnemyController enemy)
        {
            if (!enemy) return;

            float dmgMul;
            if (DamageMultiplyer <= 0f) dmgMul = 1f;
            else dmgMul = DamageMultiplyer;
            
            float spdMul;
            if (SpeedMultiplyer <= 0f) spdMul = 1f;
            else spdMul = SpeedMultiplyer;

            enemy.ApplyCombatOverrides(dmgMul, spdMul, attackAnimState, chaseAnimState);
        }
    }
}

