using UnityEngine;
using Game.Events;

namespace UI.Enemy
{
    public class EnemyUIManager : MonoBehaviour
    {
        [SerializeField] private float raycastRange = 50f;
        [SerializeField] private LayerMask enemyLayer;
        
        private Camera _mainCamera;
        private EnemyHealthBar _currentHoveredBar;
        private EventBus _eventBus;
        
        private const int EnemyLayerIndex = 6;

        private static LayerMask GetEnemyLayerMask()
        {
            var mask = LayerMask.GetMask("Enemy");
            if (mask != 0) return mask;
            mask = 1 << EnemyLayerIndex;
            return mask;
        }

        public void Initialize(EventBus eventBus)
        {
            if (_eventBus != null)
            {
                _eventBus.Unsubscribe<EntityDamagedEvent>(OnEntityDamaged);
            }

            _eventBus = eventBus;
            _eventBus.Subscribe<EntityDamagedEvent>(OnEntityDamaged);
        }

        private void Start()
        {
            _mainCamera = Camera.main;
            if (!_mainCamera) _mainCamera = FindFirstObjectByType<Camera>();
            if (enemyLayer == 0) enemyLayer = GetEnemyLayerMask();
        }
        
        private void Update()
        {
            if (!_mainCamera)
            {
                _mainCamera = Camera.main;
                if (!_mainCamera) _mainCamera = FindFirstObjectByType<Camera>();
            }
            HandleHoverRaycast();
        }
        
        private void HandleHoverRaycast()
        {
            if (!_mainCamera) return;
            if (enemyLayer == 0) return;
            
            Ray ray = _mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
            if (!Physics.Raycast(ray, out RaycastHit hit, raycastRange, enemyLayer, QueryTriggerInteraction.Ignore))
            {
                if (_currentHoveredBar)
                {
                    _currentHoveredBar.SetHovered(false);
                    _currentHoveredBar = null;
                }
                return;
            }
            
            EnemyHealthBar foundBar = hit.collider.GetComponentInParent<EnemyHealthBar>();
            if (!foundBar) foundBar = hit.collider.GetComponentInChildren<EnemyHealthBar>();
            
            if (foundBar == _currentHoveredBar) return;
            if (_currentHoveredBar) _currentHoveredBar.SetHovered(false);
            _currentHoveredBar = foundBar;
            if (_currentHoveredBar) _currentHoveredBar.SetHovered(true);
        }
        
        private void OnEntityDamaged(EntityDamagedEvent eventData)
        {
            if (eventData.Target)
            {
                var healthBar = eventData.Target.GetComponentInChildren<EnemyHealthBar>();
                if (healthBar)
                {
                    healthBar.OnDamageTaken();
                }
            }
        }
        
        private void OnDestroy()
        {
            if (_eventBus != null)
            {
                _eventBus.Unsubscribe<EntityDamagedEvent>(OnEntityDamaged);
            }
        }
    }
}
