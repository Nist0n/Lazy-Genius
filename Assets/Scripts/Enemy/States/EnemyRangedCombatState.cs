using System;
using System.Threading.Tasks;
using Player.Projectiles;
using UnityEngine;

namespace Enemy.States
{
    public class EnemyRangedCombatState : EnemyState
    {
        private float _lastShotTime;
        private float _animTimer;

        public EnemyRangedCombatState(EnemyController controller, EnemyStateMachine stateMachine, EnemyConfig config)
            : base(controller, stateMachine, config) { }

        public override void Enter()
        {
            if (controller.Agent)
            {
                controller.Agent.enabled = true;
                if (controller.Agent.isOnNavMesh)
                {
                    controller.Agent.isStopped = false;
                }
            }
            
            _animTimer = 0;
        }

        public override void Exit()
        {
            if (controller.Agent && controller.Agent.isOnNavMesh)
            {
                controller.Agent.isStopped = true;
            }
        }

        public override void LogicUpdate()
        {
            if (!controller.PlayerTransform)
            {
                stateMachine.ChangeState(controller.IdleState);
                return;
            }
            
            Vector3 toPlayer = controller.PlayerTransform.position - controller.transform.position;
            Vector3 flatDir = new Vector3(toPlayer.x, 0f, toPlayer.z);
            if (flatDir.sqrMagnitude > 0.001f)
            {
                Quaternion lookRotation = Quaternion.LookRotation(flatDir.normalized);
                controller.transform.rotation = Quaternion.Slerp(controller.transform.rotation, lookRotation, Time.deltaTime * 5f);
            }

            float distance = flatDir.magnitude;
            
            if (distance > config.DetectionRadius * 1.5f || !controller.CanSeePlayer())
            {
                stateMachine.ChangeState(controller.IdleState);
                return;
            }

            UpdateMovement(distance);
            TryShoot(distance);
        }

        private void UpdateMovement(float distance)
        {
            if (!controller.Agent || !controller.Agent.isOnNavMesh)
            {
                return;
            }

            float min = Mathf.Max(0.1f, config.PreferredMinDistance);
            float max = Mathf.Max(min + 0.1f, config.PreferredMaxDistance);
            float runAway = Mathf.Max(0.1f, config.RunAwayDistance);

            Vector3 enemyPos = controller.transform.position;
            Vector3 playerPos = controller.PlayerTransform.position;
            Vector3 dirToPlayer = (playerPos - enemyPos).normalized;
            Vector3 dirAwayFromPlayer = -dirToPlayer;

            Vector3 targetPos = enemyPos;

            if (distance < runAway)
            {
                float desiredDistance = min + (max - min) * 0.5f;
                float moveDistance = Mathf.Clamp(desiredDistance - distance, 2f, desiredDistance);
                targetPos = enemyPos + dirAwayFromPlayer * moveDistance;
                controller.Agent.isStopped = false;
            }
            else if (distance < min)
            {
                float moveDistance = Mathf.Clamp(min - distance, 1f, min);
                targetPos = enemyPos + dirAwayFromPlayer * moveDistance;
                controller.Agent.isStopped = false;
            }
            else if (distance > max)
            {
                float desiredDistance = min + (max - min) * 0.5f;
                targetPos = playerPos - dirToPlayer * desiredDistance;
                controller.Agent.isStopped = false;
            }
            else
            {
                controller.Agent.isStopped = true;
                return;
            }

            controller.Agent.SetDestination(targetPos);
            
            if (controller.Anim)
            {
                controller.Anim.Play("Chase");
            }
        }

        private void TryShoot(float distance)
        {
            if (distance < config.PreferredMinDistance || distance > config.PreferredMaxDistance)
            {
                return;
            }

            if (Time.time < _lastShotTime + config.AttackCooldown)
            {
                return;
            }

            if (!config.ProjectilePrefab)
            {
                return;
            }

            controller.Anim.Play("Attack");
            _lastShotTime = Time.time;
            StartAttack();
        }

        public void FireProjectile()
        {
            try
            {
                Vector3 origin = controller.transform.position + Vector3.up * 1.2f;
                Vector3 toPlayer = controller.PlayerTransform.position + Vector3.up * 1.0f - origin;
                if (toPlayer.sqrMagnitude < 0.001f)
                {
                    toPlayer = controller.transform.forward;
                }

                Quaternion rotation = Quaternion.LookRotation(toPlayer.normalized);
                GameObject instance = UnityEngine.Object.Instantiate(config.ProjectilePrefab, origin, rotation);

                float damage;
                if (config.ProjectileDamage > 0f) damage = config.ProjectileDamage;
                else damage = config.AttackDamage;

                var projectile = instance.GetComponent<RangedProjectile>();
                if (projectile)
                {
                    projectile.Initialize(damage, controller.gameObject);
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"EnemyRangedCombatState.FireProjectile exception: {e.Message}");
            }
        }
        
        private async void StartAttack()
        {
            try
            {
                while (_animTimer <= 2)
                {
                    _animTimer += Time.deltaTime;
                    await Task.Yield();
                }
                
                stateMachine.ChangeState(controller.RangedCombatState);
            }
            catch (Exception e)
            {
                Debug.LogWarning(e.Message);
            }
        }
    }
}

