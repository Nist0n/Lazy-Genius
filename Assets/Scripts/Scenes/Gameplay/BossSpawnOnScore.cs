using Enemy;
using Game.Events;
using UnityEngine;

namespace Scenes.Gameplay
{
    public sealed class BossSpawnOnScore : MonoBehaviour
    {
        [SerializeField] private GameObject bossPrefab;
        [SerializeField] private Transform spawnPoint;
        [SerializeField] [Min(1)] private int killsToSpawnBoss = 3;

        private bool _spawned;
        private EventBus _eventBus;

        public void Initialize(EventBus eventBus)
        {
            _eventBus = eventBus;
        }

        public void OnScoreChanged(int score)
        {
            if (_spawned || score < killsToSpawnBoss || !bossPrefab)
            {
                return;
            }

            Vector3 position;
            if (spawnPoint) position = spawnPoint.position;
            else position = transform.position;
            
            Quaternion rotation;
            if (spawnPoint) rotation = spawnPoint.rotation;
            else rotation = transform.rotation;
            
            var boss = Instantiate(bossPrefab, position, rotation);
            if (_eventBus != null && boss.TryGetComponent(out EnemyHealth health))
            {
                health.BindEventBus(_eventBus);
            }

            _spawned = true;
        }
    }
}
