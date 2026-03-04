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
        
        private const int EnemyLayerIndex = 6;

        private static LayerMask GetEnemyLayerMask()
        {
            var mask = LayerMask.GetMask("Enemy");
            if (mask != 0) return mask;
            mask = 1 << EnemyLayerIndex;
            return mask;
        }

        private void Start()
        {
            _mainCamera = Camera.main;
            if (!_mainCamera) _mainCamera = FindFirstObjectByType<Camera>();
            if (enemyLayer == 0) enemyLayer = GetEnemyLayerMask();
            GameEvents.EntityDamaged += OnEntityDamaged;
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
        
        private void OnEntityDamaged(GameObject target, float amount)
        {
            if (target)
            {
                var healthBar = target.GetComponentInChildren<EnemyHealthBar>();
                if (healthBar)
                {
                    healthBar.OnDamageTaken();
                }
            }
        }
        
        private void OnDestroy()
        {
            GameEvents.EntityDamaged -= OnEntityDamaged;
        }
    }
}
