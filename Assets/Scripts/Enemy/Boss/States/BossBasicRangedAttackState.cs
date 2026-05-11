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
            controller.PlayBossBasicAttackAnimation();
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

            for (int i = 0; i < shotCount; i++)
            {
                if (controller.IsDead || !controller.PlayerTransform)
                {
                    stateMachine.ChangeState(controller.IdleState);
                    yield break;
                }

                float aimDelay = controller.GetEffectiveBasicAimDelay();
                yield return new WaitForSeconds(aimDelay);

                Vector3 target = controller.PlayerTransform.position + Vector3.up;
                GameObject prefab = controller.GetBasicProjectilePrefab();
                controller.FireProjectileAtTarget(prefab, controller.GetEffectiveBasicDamage(), target);

                yield return new WaitForSeconds(controller.GetEffectiveBasicInterval());
            }

            stateMachine.ChangeState(controller.ChaseState);
        }
    }
}
