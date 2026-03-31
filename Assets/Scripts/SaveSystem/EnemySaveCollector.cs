using System.Collections.Generic;
using Enemy;
using UnityEngine;

namespace SaveSystem
{
    public class EnemySaveCollector
    {
        public List<EnemySaveData> Collect()
        {
            var result = new List<EnemySaveData>();

            var enemies = Object.FindObjectsByType<EnemyController>(FindObjectsSortMode.None);
            if (enemies == null || enemies.Length == 0)
            {
                return result;
            }

            foreach (var enemy in enemies)
            {
                if (!enemy) continue;
                if (!enemy.gameObject.activeInHierarchy) continue;
                
                if (!enemy.enabled) continue;

                string key = GetEnemyKey(enemy.transform);

                result.Add(new EnemySaveData
                {
                    key = key,
                    position = enemy.transform.position,
                    rotation = enemy.transform.rotation
                });
            }

            return result;
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

