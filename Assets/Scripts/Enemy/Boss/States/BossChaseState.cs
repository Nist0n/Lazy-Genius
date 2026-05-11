using UnityEngine;

namespace Enemy.Boss.States
{
    public class BossChaseState : BossState
    {
        public BossChaseState(BossController controller, BossStateMachine stateMachine, BossConfig config) : base(controller, stateMachine, config)
        {
        }

        public override void Enter()
        {
            controller.SetMovementEnabled(true);
            controller.PlayBossChaseAnimation();
        }

        public override void Exit()
        {
            controller.SetMovementEnabled(false);
        }

        public override void LogicUpdate()
        {
            if (controller.EnragedPending)
            {
                stateMachine.ChangeState(controller.EnragedState);
                return;
            }

            if (!controller.PlayerTransform)
            {
                stateMachine.ChangeState(controller.IdleState);
                return;
            }

            controller.LookAtPlayer();
            controller.MoveTo(controller.PlayerTransform.position);

            float distance = controller.DistanceToPlayer();
            bool canAttack = controller.CanSeePlayer() && distance >= config.CombatMinDistance && distance <= config.CombatMaxDistance;
            if (!canAttack)
            {
                if (distance > config.DetectionRadius)
                {
                    stateMachine.ChangeState(controller.IdleState);
                }

                return;
            }
            
            float roll = Random.value;
            if (controller.IsRocketReady() && roll < 0.3f)
            {
                stateMachine.ChangeState(controller.RocketBarrageState);
                return;
            }

            if (roll < 0.65f)
            {
                stateMachine.ChangeState(controller.BasicAttackState);
            }
            else
            {
                stateMachine.ChangeState(controller.SuppressiveFireState);
            }
        }
    }
}
