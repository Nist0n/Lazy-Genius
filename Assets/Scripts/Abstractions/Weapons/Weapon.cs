using UnityEngine;

namespace Abstractions.Weapons
{
    public abstract class Weapon : MonoBehaviour
    {
        public float DamageMultiplyer;
        public float SpeedMultiplyer;
        public WeaponType TypeOfWeapon;
    }

    public enum WeaponType
    {
        Ranged,
        Melee,
        Special
    }
}
