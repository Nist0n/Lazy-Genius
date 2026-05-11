using UnityEngine;

namespace Enemy.Weapons
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(EnemyController))]
    public class EnemyWeaponSpawner : MonoBehaviour
    {
        [Header("Spawn Point")]
        [SerializeField] private Transform weaponSocket;

        [Header("Loadout")]
        [SerializeField] private EnemyWeaponLoadout loadout;
        [SerializeField] private EnemyWeapon[] weaponPrefabs;

        [Range(0f, 1f)]
        [SerializeField] private float noWeaponChance;

        private EnemyController _enemy;
        private EnemyWeapon _spawnedWeapon;

        private void Awake()
        {
            _enemy = GetComponent<EnemyController>();
        }

        private void Start()
        {
            SpawnAndApply();
        }

        public void SpawnAndApply()
        {
            if (!_enemy) _enemy = GetComponent<EnemyController>();
            if (!_enemy) return;

            ClearSpawnedWeapon();
            _enemy.ResetCombatOverrides();

            if (!TryPickWeapon(out var weaponPrefab))
            {
                return;
            }

            Transform parent;
            if (weaponSocket) parent = weaponSocket;
            else parent = transform;
            _spawnedWeapon = Instantiate(weaponPrefab, parent.position, parent.rotation, parent);
            _spawnedWeapon.ApplyTo(_enemy);
        }

        private bool TryPickWeapon(out EnemyWeapon weaponPrefab)
        {
            weaponPrefab = null;

            if (loadout)
            {
                return loadout.TryPick(out weaponPrefab);
            }

            if (noWeaponChance > 0f && Random.value < noWeaponChance)
            {
                return false;
            }

            if (weaponPrefabs == null || weaponPrefabs.Length == 0)
            {
                return false;
            }

            int safety = 32;
            while (safety-- > 0)
            {
                int idx = Random.Range(0, weaponPrefabs.Length);
                if (idx < 0 || idx >= weaponPrefabs.Length) continue;
                var candidate = weaponPrefabs[idx];
                if (!candidate) continue;
                weaponPrefab = candidate;
                return true;
            }

            return false;
        }

        private void ClearSpawnedWeapon()
        {
            if (!_spawnedWeapon) return;
            Destroy(_spawnedWeapon.gameObject);
            _spawnedWeapon = null;
        }
    }
}

