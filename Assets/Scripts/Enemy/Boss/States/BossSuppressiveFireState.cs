using System.Collections;
using UnityEngine;

namespace Enemy.Boss.States
{
    public class BossSuppressiveFireState : BossState
    {
        private Coroutine _fireRoutine;

        public BossSuppressiveFireState(BossController controller, BossStateMachine stateMachine, BossConfig config) : base(controller, stateMachine, config)
        {
        }

        public override void Enter()
        {
            controller.SetMovementEnabled(false);
            controller.PlayBossSuppressiveAttackAnimation();
            _fireRoutine = controller.StartCoroutine(SuppressiveFireRoutine());
        }

        public override void Exit()
        {
            if (_fireRoutine != null)
            {
                controller.StopCoroutine(_fireRoutine);
                _fireRoutine = null;
            }
        }

        private IEnumerator SuppressiveFireRoutine()
        {
            float duration = config.SuppressiveDuration + (controller.IsEnraged ? config.EnragedSuppressiveBonusDuration : 0f);
            float shotInterval = controller.GetEffectiveSuppressiveShotInterval();
            float elapsed = 0f;

            while (elapsed < duration)
            {
                if (controller.IsDead || !controller.PlayerTransform)
                {
                    stateMachine.ChangeState(controller.IdleState);
                    yield break;
                }

                if (!controller.CanSeePlayer())
                {
                    stateMachine.ChangeState(controller.ChaseState);
                    yield break;
                }

                controller.LookAtPlayer();
                Vector3 muzzleOrigin = controller.GetMuzzlePosition();
                Vector3 baseDirection = (controller.PlayerTransform.position + Vector3.up - muzzleOrigin).normalized;
                Vector3 spreadDirection = Quaternion.Euler(
                    Random.Range(-config.SuppressiveSpreadAngle, config.SuppressiveSpreadAngle),
                    Random.Range(-config.SuppressiveSpreadAngle, config.SuppressiveSpreadAngle),
                    0f
                ) * baseDirection;

                GameObject prefab = controller.GetSuppressiveProjectilePrefab();
                controller.FireProjectile(prefab, controller.GetEffectiveSuppressiveDamage(), spreadDirection);
                yield return new WaitForSeconds(shotInterval);
                elapsed += shotInterval;
            }

            stateMachine.ChangeState(controller.OverheatState);
        }
    }
}
