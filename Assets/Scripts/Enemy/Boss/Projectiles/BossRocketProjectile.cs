using Core;
using UnityEngine;

namespace Enemy.Boss.Projectiles
{
    public class BossRocketProjectile : MonoBehaviour
    {
        [SerializeField] private float proximityThreshold = 0.35f;

        private float _speed;
        private float _damage;
        private float _explosionRadius;
        private float _lifetime;
        private GameObject _source;
        private Vector3 _targetPosition;
        private bool _isInitialized;

        public void Initialize(
            Vector3 targetPosition,
            float speed,
            float damage,
            float explosionRadius,
            float lifetime,
            GameObject source)
        {
            _targetPosition = targetPosition;
            _speed = Mathf.Max(0.1f, speed);
            _damage = Mathf.Max(0f, damage);
            _explosionRadius = Mathf.Max(0.1f, explosionRadius);
            _lifetime = Mathf.Max(0.1f, lifetime);
            _source = source;
            _isInitialized = true;
        }

        private void Update()
        {
            if (!_isInitialized)
            {
                return;
            }

            _lifetime -= Time.deltaTime;
            if (_lifetime <= 0f)
            {
                Explode();
                return;
            }

            Vector3 toTarget = _targetPosition - transform.position;
            float distance = toTarget.magnitude;
            if (distance <= proximityThreshold)
            {
                Explode();
                return;
            }

            Vector3 moveDirection = toTarget.normalized;
            transform.position += moveDirection * (_speed * Time.deltaTime);

            if (moveDirection.sqrMagnitude > 0.0001f)
            {
                transform.rotation = Quaternion.LookRotation(moveDirection);
            }
        }

        private void Explode()
        {
            Collider[] hits = Physics.OverlapSphere(transform.position, _explosionRadius, Physics.AllLayers, QueryTriggerInteraction.Collide);

            for (int i = 0; i < hits.Length; i++)
            {
                Collider hit = hits[i];
                if (!hit)
                {
                    continue;
                }

                if (_source && hit.transform.root == _source.transform)
                {
                    continue;
                }

                IDamageable damageable = hit.GetComponentInParent<IDamageable>();
                if (damageable == null)
                {
                    continue;
                }

                var info = new DamageInfo(
                    _damage,
                    DamageSourceType.Ability,
                    _source,
                    hit.ClosestPoint(transform.position),
                    Vector3.up);

                damageable.TakeDamage(_damage, info);
            }

            Destroy(gameObject);
        }
    }
}
