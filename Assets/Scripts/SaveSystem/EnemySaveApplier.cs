using System.Collections.Generic;
using Enemy;
using UnityEngine;

namespace SaveSystem
{
    public sealed class EnemySaveApplier
    {
        public void Apply(List<EnemySaveData> savedEnemies)
        {
            if (savedEnemies == null || savedEnemies.Count == 0)
            {
                return;
            }

            var enemies = Object.FindObjectsByType<EnemyController>(FindObjectsSortMode.None);
            if (enemies == null || enemies.Length == 0)
            {
                return;
            }

            var map = new Dictionary<string, EnemySaveData>(savedEnemies.Count);
            foreach (var data in savedEnemies)
            {
                if (data == null) continue;
                if (string.IsNullOrWhiteSpace(data.key)) continue;
                map[data.key] = data;
            }

            foreach (var enemy in enemies)
            {
                if (!enemy) continue;

                string key = GetEnemyKey(enemy.transform);
                if (string.IsNullOrWhiteSpace(key)) continue;

                if (!map.TryGetValue(key, out var data)) continue;
                
                if (enemy.Agent && enemy.Agent.enabled)
                {
                    enemy.Agent.Warp(data.position);
                }
                else
                {
                    enemy.transform.position = data.position;
                }

                enemy.transform.rotation = data.rotation;
            }
        }

        private static string GetEnemyKey(Transform t)
        {
            var persistentId = t.GetComponent<EnemyPersistentId>();
            if (persistentId && !string.IsNullOrWhiteSpace(persistentId.Id))
            {
                return persistentId.Id;
            }

            return GetHierarchyPath(t);
        }

        private static string GetHierarchyPath(Transform t)
        {
            if (!t) return string.Empty;

            string path = t.name;
            Transform current = t.parent;
            while (current)
            {
                path = current.name + "/" + path;
                current = current.parent;
            }

            return path;
        }
    }
}

