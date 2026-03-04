using Core;
using UnityEngine;

namespace Player.Projectiles
{
    [RequireComponent(typeof(Collider))]
    public class RangedProjectile : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float speed = 25f;
        [SerializeField] private float maxLifetime = 3f;

        private float _damage;
        private GameObject _source;
        private float _lifetime;

        private void Awake()
        {
            _lifetime = maxLifetime;
        }

        public void Initialize(float damage, GameObject source)
        {
            _damage = damage;
            _source = source;
        }

        private void Update()
        {
            float delta = Time.deltaTime;
            transform.position += transform.forward * (speed * delta);

            _lifetime -= delta;
            if (_lifetime <= 0f)
            {
                Destroy(gameObject);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other)
            {
                return;
            }

            if (_source && other.attachedRigidbody && other.attachedRigidbody.gameObject == _source)
            {
                return;
            }

            var damageable = other.GetComponentInParent<IDamageable>();
            if (damageable == null)
            {
                return;
            }

            Vector3 impactPoint = other.ClosestPoint(transform.position);
            Vector3 impactNormal = -transform.forward;

            var info = new DamageInfo(_damage, DamageSourceType.Ability, _source, impactPoint, impactNormal);
            damageable.TakeDamage(_damage, info);

            Destroy(gameObject);
        }
    }
}

