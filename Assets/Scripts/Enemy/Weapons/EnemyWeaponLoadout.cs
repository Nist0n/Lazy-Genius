using System;
using System.Collections.Generic;
using UnityEngine;

namespace Enemy.Weapons
{
    [CreateAssetMenu(fileName = "EnemyWeaponLoadout", menuName = "Lazy-Genius/Enemy/Weapon Loadout")]
    public class EnemyWeaponLoadout : ScriptableObject
    {
        [Range(0f, 1f)]
        public float NoWeaponChance = 0f;
        public List<EnemyWeapon> WeaponPrefabs = new();

        public bool TryPick(out EnemyWeapon weaponPrefab)
        {
            weaponPrefab = null;

            if (NoWeaponChance > 0f && UnityEngine.Random.value < NoWeaponChance)
            {
                return false;
            }

            if (WeaponPrefabs == null || WeaponPrefabs.Count == 0)
            {
                return false;
            }

            int safety = 32;
            while (safety-- > 0)
            {
                int idx = UnityEngine.Random.Range(0, WeaponPrefabs.Count);
                if (idx < 0 || idx >= WeaponPrefabs.Count) continue;
                var candidate = WeaponPrefabs[idx];
                if (!candidate) continue;
                weaponPrefab = candidate;
                return true;
            }

            return false;
        }
    }
}

