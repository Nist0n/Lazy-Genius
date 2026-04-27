using System.Collections;
using UnityEngine;

namespace Enemy.Boss.States
{
    public class BossBasicRangedAttackState : BossState
    {
        private Coroutine _attackRoutine;

        public BossBasicRangedAttackState(BossController controller, BossStateMachine stateMachine, BossConfig config) : base(controller, stateMachine, config)
        {
        }

        public override void Enter()
        {
            controller.SetMovementEnabled(false);
            controller.PlayAnimation("Attack");
            _attackRoutine = controller.StartCoroutine(AttackRoutine());
        }

        public override void Exit()
        {
            if (_attackRoutine != null)
            {
                controller.StopCoroutine(_attackRoutine);
                _attackRoutine = null;
            }
        }

        public override void LogicUpdate()
        {
            if (controller.EnragedPending)
            {
                stateMachine.ChangeState(controller.EnragedState);
            }
            controller.LookAtPlayer();
        }

        private IEnumerator AttackRoutine()
        {
            int minShots = Mathf.Max(1, config.BasicShotCountRange.x);
            int maxShots = Mathf.Max(minShots, config.BasicShotCountRange.y);
            int shotCount = Random.Range(minShots, maxShots + 1);
            float aimDelay;
            if (controller.IsEnraged) aimDelay = config.EnragedBasicAimDelay;
            else aimDelay = config.BasicAimDelay;

            for (int i = 0; i < shotCount; i++)
            {
                if (controller.IsDead || !controller.PlayerTransform)
                {
                    stateMachine.ChangeState(controller.IdleState);
                    yield break;
                }
                
                yield return new WaitForSeconds(aimDelay);

                Vector3 target = controller.PlayerTransform.position + Vector3.up;
                controller.FireProjectileAtTarget(config.BasicProjectilePrefab, config.BasicAttackDamage, target);

                yield return new WaitForSeconds(config.BasicInterval);
            }

            stateMachine.ChangeState(controller.ChaseState);
        }
    }
}
