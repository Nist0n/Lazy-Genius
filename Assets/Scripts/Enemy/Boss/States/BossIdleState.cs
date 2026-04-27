using UnityEngine;

namespace Enemy.Boss.States
{
    public class BossIdleState : BossState
    {
        public BossIdleState(BossController controller, BossStateMachine stateMachine, BossConfig config) : base(controller, stateMachine, config)
        {
        }

        public override void Enter()
        {
            controller.SetMovementEnabled(false);
            controller.PlayAnimation("Idle");
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
                return;
            }

            float distance = controller.DistanceToPlayer();
            
            if (!controller.CanSeePlayer())
            {
                return;
            }

            if (controller.IsPeacefulModeEnabled)
            {
                return;
            }

            if (distance > config.ChaseMinDistance && distance < config.DetectionRadius)
            {
                stateMachine.ChangeState(controller.ChaseState);
                return;
            }

            if (distance >= config.CombatMinDistance && distance <= config.CombatMaxDistance)
            {
                stateMachine.ChangeState(controller.BasicAttackState);
            }
        }
    }
}
