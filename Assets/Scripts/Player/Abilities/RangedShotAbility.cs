using Player.Projectiles;
using UnityEngine;

namespace Player.Abilities
{
    [CreateAssetMenu(fileName = "RangedShotAbility", menuName = "Lazy-Genius/Player/Abilities/Ranged Shot")]
    public class RangedShotAbility : Ability
    {
        [Header("Projectile Settings")]
        [SerializeField] private GameObject projectilePrefab;
        [SerializeField] private float projectileDamage = 15f;
        
        [Header("Aim (Crosshair Raycast)")]
        [SerializeField] private float aimMaxDistance = 500f;
        [SerializeField] private LayerMask aimRaycastMask = ~0;

        private void OnEnable()
        {
            cooldown = 2f;
            energyCost = 5f;
            isChanneled = false;
        }

        public override void Activate(GameObject caster)
        {
            if (!caster) return;

            var player = caster.GetComponent<PlayerController>();
            if (!player) return;

            player.PlAnimator?.PlayAbilityAnimation(PlayerAnimator.AbilityAnimationType.RangedShot);

            if (!projectilePrefab) return;

            Transform spawnPoint = player.ProjectileSpawnPoint;
            if (!spawnPoint)
            {
                spawnPoint = player.transform;
            }
            
            Camera cam;
            if (player.MainCamera) cam = player.MainCamera;
            else cam = Camera.main;
            Vector3 targetPoint;

            if (cam)
            {
                Ray aimRay = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
                if (Physics.Raycast(aimRay, out RaycastHit hit, aimMaxDistance, aimRaycastMask, QueryTriggerInteraction.Ignore))
                {
                    targetPoint = hit.point;
                }
                else
                {
                    targetPoint = aimRay.origin + aimRay.direction * aimMaxDistance;
                }
            }
            else
            {
                targetPoint = spawnPoint.position + player.transform.forward * aimMaxDistance;
            }

            Vector3 direction = (targetPoint - spawnPoint.position);
            if (direction.sqrMagnitude < 0.001f)
            {
                direction = player.transform.forward;
            }
            direction.Normalize();

            Quaternion rotation = Quaternion.LookRotation(direction);
            GameObject projectileInstance = Instantiate(projectilePrefab, spawnPoint.position, rotation);

            var projectile = projectileInstance.GetComponent<RangedProjectile>();
            if (projectile)
            {
                projectile.Initialize(projectileDamage, caster);
            }
        }
    }
}

