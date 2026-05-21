using System;
using System.Collections;
using Player.Projectiles;
using UnityEngine;

namespace Enemy
{
    public class EnemyRangedController : EnemyController
    {
        [SerializeField] private string defaultChaseAnimState = "Chase";

        private string _chaseAnimOverride;
        private float _lastRangedShotTime;
        private float _rangedAnimTimer;
        private Coroutine _rangedAttackRoutine;

        public float EffectiveProjectileDamage
        {
            get
            {
                if (!EnemyConfig) return EffectiveAttackDamage;

                float baseDamage = EnemyConfig.ProjectileDamage > 0f
                    ? EnemyConfig.ProjectileDamage
                    : EnemyConfig.AttackDamage;

                return baseDamage * Mathf.Max(0f, DamageMultiplier);
            }
        }

        public string ChaseAnimState =>
            string.IsNullOrWhiteSpace(_chaseAnimOverride) ? defaultChaseAnimState : _chaseAnimOverride;

        protected override EnemyFightStateMachine CreateFightStateMachine() =>
            new EnemyRangedFightStateMachine(this);

        protected override void StopCombatRoutines() => StopRangedAttackRoutine();

        public override bool ShouldChaseAfterHit() => false;

        public override bool ShouldCombatAfterHit()
        {
            if (IsPeacefulModeEnabled) return false;
            return true;
        }

        public override void ResetCombatOverrides()
        {
            base.ResetCombatOverrides();
            _chaseAnimOverride = defaultChaseAnimState;
        }

        public override void ApplyCombatOverrides(
            float damageMultiplier,
            float attackSpeedMultiplier,
            string attackAnimState,
            string chaseAnimState = null)
        {
            base.ApplyCombatOverrides(damageMultiplier, attackSpeedMultiplier, attackAnimState, chaseAnimState);
            _chaseAnimOverride = chaseAnimState;
        }

        public void EnterRangedCombat()
        {
            if (Agent)
            {
                Agent.enabled = true;
                if (Agent.isOnNavMesh)
                {
                    Agent.isStopped = false;
                }
            }

            _rangedAnimTimer = 0f;
        }

        public void ExitRangedCombat()
        {
            if (Agent && Agent.isOnNavMesh)
            {
                Agent.isStopped = true;
            }

            StopRangedAttackRoutine();
        }

        public void StopRangedAttackRoutine()
        {
            if (_rangedAttackRoutine != null)
            {
                StopCoroutine(_rangedAttackRoutine);
                _rangedAttackRoutine = null;
            }
        }

        public bool ShouldLosePlayerFromRangedCombat()
        {
            return GetDistanceToPlayer() > GetDetectionRadius() * 1.5f || !CanSeePlayer();
        }

        public void UpdateRangedCombatMovement()
        {
            if (!PlayerTransform || !Agent || !Agent.isOnNavMesh || !EnemyConfig)
            {
                return;
            }

            float distance = GetFlatDistanceToPlayer();
            float min = Mathf.Max(0.1f, EnemyConfig.PreferredMinDistance);
            float max = Mathf.Max(min + 0.1f, EnemyConfig.PreferredMaxDistance);
            float runAway = Mathf.Max(0.1f, EnemyConfig.RunAwayDistance);

            Vector3 enemyPos = transform.position;
            Vector3 playerPos = PlayerTransform.position;
            Vector3 dirToPlayer = (playerPos - enemyPos).normalized;
            Vector3 dirAwayFromPlayer = -dirToPlayer;
            Vector3 targetPos = enemyPos;

            if (distance < runAway)
            {
                float desiredDistance = min + (max - min) * 0.5f;
                float moveDistance = Mathf.Clamp(desiredDistance - distance, 2f, desiredDistance);
                targetPos = enemyPos + dirAwayFromPlayer * moveDistance;
                Agent.isStopped = false;
            }
            else if (distance < min)
            {
                float moveDistance = Mathf.Clamp(min - distance, 1f, min);
                targetPos = enemyPos + dirAwayFromPlayer * moveDistance;
                Agent.isStopped = false;
            }
            else if (distance > max)
            {
                float desiredDistance = min + (max - min) * 0.5f;
                targetPos = playerPos - dirToPlayer * desiredDistance;
                Agent.isStopped = false;
            }
            else
            {
                Agent.isStopped = true;
                return;
            }

            Agent.SetDestination(targetPos);
            if (Anim) Anim.Play(ChaseAnimState);
        }

        public bool TryStartRangedShot()
        {
            if (!EnemyConfig || !EnemyConfig.ProjectilePrefab || !PlayerTransform)
            {
                return false;
            }

            float distance = GetFlatDistanceToPlayer();
            if (distance < EnemyConfig.PreferredMinDistance || distance > EnemyConfig.PreferredMaxDistance)
            {
                return false;
            }

            if (Time.time < _lastRangedShotTime + EffectiveAttackCooldown)
            {
                return false;
            }

            if (Anim) Anim.Play(AttackAnimState);
            _lastRangedShotTime = Time.time;
            StopRangedAttackRoutine();
            _rangedAttackRoutine = StartCoroutine(RangedAttackRoutine());
            return true;
        }

        public void UpdateRangedCombatFacing()
        {
            LookAtPlayerFlat(5f);
        }

        private IEnumerator RangedAttackRoutine()
        {
            _rangedAnimTimer = 0f;

            while (_rangedAnimTimer <= 0.5f)
            {
                _rangedAnimTimer += Time.deltaTime;
                yield return null;
            }

            FireRangedProjectile();

            while (_rangedAnimTimer <= 2f)
            {
                _rangedAnimTimer += Time.deltaTime;
                yield return null;
            }

            _rangedAttackRoutine = null;
            FightStateMachine.ChangeState(FightStateMachine.CreateCombatState());
        }

        private void FireRangedProjectile()
        {
            if (!EnemyConfig?.ProjectilePrefab || !PlayerTransform)
            {
                return;
            }

            try
            {
                Vector3 origin = transform.position + Vector3.up * 1.2f;
                Vector3 toPlayer = PlayerTransform.position + Vector3.up * 1.0f - origin;
                if (toPlayer.sqrMagnitude < 0.001f)
                {
                    toPlayer = transform.forward;
                }

                Quaternion rotation = Quaternion.LookRotation(toPlayer.normalized);
                GameObject instance = Instantiate(EnemyConfig.ProjectilePrefab, origin, rotation);

                var projectile = instance.GetComponent<RangedProjectile>();
                if (projectile)
                {
                    projectile.Initialize(EffectiveProjectileDamage, gameObject);
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"EnemyRangedController.FireRangedProjectile: {e.Message}");
            }
        }
    }
}
