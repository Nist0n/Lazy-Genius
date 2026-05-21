using System.Collections.Generic;
using Enemy;
using Game.Events;
using SaveSystem;
using UnityEngine;

namespace Scenes.Gameplay
{
    public sealed class EnemyRuntimeSpawner : MonoBehaviour
    {
        private const int OverlapBufferSize = 32;
        private static readonly Collider[] OverlapBuffer = new Collider[OverlapBufferSize];

        [Header("Spawn Rules")]
        [SerializeField] [Min(0)] private int enemyCount = 4;
        [SerializeField] private EnemyController[] enemyPrefabs = System.Array.Empty<EnemyController>();

        [Header("Random Area")]
        [SerializeField] private Transform areaCornerA;
        [SerializeField] private Transform areaCornerB;

        [Header("Placement")]
        [SerializeField] private LayerMask environmentMask;
        [SerializeField] [Min(0.01f)] private float spawnClearanceRadius = 0.45f;
        [SerializeField] [Min(1)] private int maxRandomPlacementAttempts = 64;
        [SerializeField] private Transform spawnedRoot;

        private bool _spawned;
        private EventBus _eventBus;

        public void Initialize(EventBus eventBus)
        {
            _eventBus = eventBus;
        }

        private void Reset()
        {
            environmentMask = LayerMask.GetMask("Environment");
        }

        private void Awake()
        {
            if (environmentMask.value == 0)
            {
                environmentMask = LayerMask.GetMask("Environment");
            }

            if (!spawnedRoot)
            {
                var root = new GameObject("SpawnedEnemies");
                root.transform.SetParent(transform, false);
                spawnedRoot = root.transform;
            }
        }
        
        public void TrySpawnFromActiveCharacter()
        {
            if (_spawned)
            {
                return;
            }

            if (enemyCount <= 0 || enemyPrefabs == null || enemyPrefabs.Length == 0)
            {
                return;
            }

            for (var i = 0; i < enemyPrefabs.Length; i++)
            {
                if (!enemyPrefabs[i])
                {
                    return;
                }
            }

            if (!CharacterManager.Instance || !CharacterManager.Instance.HasActiveCharacter)
            {
                return;
            }

            var character = CharacterManager.Instance.ActiveCharacter;
            if (character == null)
            {
                return;
            }

            ClearSpawnedChildren();

            var saved = character.Enemies;
            var useSaved = character.HasGameplayState && saved != null && saved.Count == enemyCount && enemyCount > 0;

            if (useSaved)
            {
                SpawnFromSave(saved);
            }
            else
            {
                SpawnRandomAndSyncCharacterEnemies(character);
            }

            _spawned = true;
        }

        private void ClearSpawnedChildren()
        {
            if (!spawnedRoot)
            {
                return;
            }

            for (var i = spawnedRoot.childCount - 1; i >= 0; i--)
            {
                var child = spawnedRoot.GetChild(i);
                if (child)
                {
                    Destroy(child.gameObject);
                }
            }
        }

        private void SpawnFromSave(IReadOnlyList<EnemySaveData> savedEnemies)
        {
            for (var i = 0; i < enemyCount; i++)
            {
                var data = savedEnemies[i];
                if (data == null || string.IsNullOrWhiteSpace(data.key))
                {
                    continue;
                }

                var prefab = enemyPrefabs[i % enemyPrefabs.Length];
                var instance = Instantiate(prefab, data.position, data.rotation, spawnedRoot);
                ApplySavedKey(instance, data.key);
                PlaceSpawnedEnemy(instance, data.position);
            }
        }

        private void SpawnRandomAndSyncCharacterEnemies(CharacterData character)
        {
            if (!areaCornerA || !areaCornerB)
            {
                return;
            }

            var min = Vector3.Min(areaCornerA.position, areaCornerB.position);
            var max = Vector3.Max(areaCornerA.position, areaCornerB.position);

            for (var i = 0; i < enemyCount; i++)
            {
                var prefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
                if (!TryFindSpawnPoint(min, max, out var spawnPosition, out var spawnRotation))
                {
                    continue;
                }

                var instance = Instantiate(prefab, spawnPosition, spawnRotation, spawnedRoot);
                EnsureRuntimePersistentId(instance);
                PlaceSpawnedEnemy(instance, spawnPosition);
            }

            var collector = new EnemySaveCollector();
            character.Enemies = collector.Collect();
        }

        private static void ApplySavedKey(EnemyController instance, string key)
        {
            if (!instance)
            {
                return;
            }

            var pid = instance.GetComponent<EnemyPersistentId>();
            if (!pid)
            {
                pid = instance.gameObject.AddComponent<EnemyPersistentId>();
            }

            pid.SetId(key);
        }

        private static void EnsureRuntimePersistentId(EnemyController instance)
        {
            if (!instance)
            {
                return;
            }

            var pid = instance.GetComponent<EnemyPersistentId>();
            if (!pid)
            {
                pid = instance.gameObject.AddComponent<EnemyPersistentId>();
            }

            pid.SetId(System.Guid.NewGuid().ToString("N"));
        }

        private bool TryFindSpawnPoint(
            Vector3 boundsMin,
            Vector3 boundsMax,
            out Vector3 position,
            out Quaternion rotation)
        {
            position = default;
            rotation = Quaternion.identity;

            for (var attempt = 0; attempt < maxRandomPlacementAttempts; attempt++)
            {
                var candidate = PickRandomSpawnCandidate(boundsMin, boundsMax);
                var checkCenter = candidate + Vector3.up;

                var overlapCount = Physics.OverlapSphereNonAlloc(
                    checkCenter,
                    spawnClearanceRadius,
                    OverlapBuffer,
                    environmentMask,
                    QueryTriggerInteraction.Ignore);

                if (overlapCount > 0)
                {
                    continue;
                }

                position = candidate;
                rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
                return true;
            }

            return false;
        }

        private Vector3 PickRandomSpawnCandidate(Vector3 boundsMin, Vector3 boundsMax)
        {
            var x = Random.Range(boundsMin.x, boundsMax.x);
            var z = Random.Range(boundsMin.z, boundsMax.z);

            var ry = Random.Range(boundsMin.y, boundsMax.y);
            return new Vector3(x, ry, z);
        }

        private void PlaceSpawnedEnemy(EnemyController enemy, Vector3 position)
        {
            if (!enemy)
            {
                return;
            }

            enemy.transform.position = position;
            enemy.Health?.BindEventBus(_eventBus);
        }
    }
}
