using UnityEngine;

namespace Enemy.Boss.States
{
    public class BossEnragedState : BossState
    {
        public BossEnragedState(BossController controller, BossStateMachine stateMachine, BossConfig config) : base(controller, stateMachine, config)
        {
        }

        public override void Enter()
        {
            base.Enter();
            controller.EnragedPending = false;
            controller.ApplyEnragedModifiers();
            controller.SetMovementEnabled(false);
            controller.PlayAnimation("Enraged");
        }

        public override void LogicUpdate()
        {
            if (Time.time >= startTime + config.EnragedStateDuration)
            {
                stateMachine.ChangeState(controller.ChaseState);
            }
        }
    }
}
