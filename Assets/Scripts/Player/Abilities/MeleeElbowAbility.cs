using Core;
using UnityEngine;

namespace Player.Abilities
{
    [CreateAssetMenu(fileName = "MeleeElbowAbility", menuName = "Lazy-Genius/Player/Abilities/Melee Elbow")]
    public class MeleeElbowAbility : Ability
    {
        [Header("Melee Settings")]
        [SerializeField] private float damage = 10f;
        [SerializeField] private float hitRange = 1.5f;
        [SerializeField] private float hitRadius = 0.75f;
        [SerializeField] private LayerMask hitMask = ~0;

        private void OnEnable()
        {
            cooldown = 1f;
            energyCost = 0f;
            isChanneled = false;
        }

        public override void Activate(GameObject caster)
        {
            if (!caster) return;

            var player = caster.GetComponent<PlayerController>();
            if (!player) return;

            player.PlAnimator?.PlayAbilityAnimation(PlayerAnimator.AbilityAnimationType.MeleeElbow);

            Transform originTransform = player.transform;
            Vector3 forward = originTransform.forward;

            Vector3 origin = originTransform.position + Vector3.up * 1.0f + forward * hitRange;

            Collider[] hits = Physics.OverlapSphere(origin, hitRadius, hitMask, QueryTriggerInteraction.Ignore);

            for (int i = 0; i < hits.Length; i++)
            {
                Collider hit = hits[i];

                if (!hit || hit.attachedRigidbody && hit.attachedRigidbody.gameObject == caster)
                {
                    continue;
                }

                var damageable = hit.GetComponentInParent<IDamageable>();
                if (damageable == null)
                {
                    continue;
                }

                if (ReferenceEquals(damageable, player.HealthSystem))
                {
                    continue;
                }

                Vector3 impactPoint = hit.ClosestPoint(origin);
                Vector3 impactNormal = -forward;

                var info = new DamageInfo(damage, DamageSourceType.Ability, caster, impactPoint, impactNormal);
                damageable.TakeDamage(damage, info);
            }
        }
    }
}

