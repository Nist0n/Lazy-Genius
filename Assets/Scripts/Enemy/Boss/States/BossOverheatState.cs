using UnityEngine;

namespace Enemy.Boss.States
{
    public class BossOverheatState : BossState
    {
        private float _cooldownDuration;

        public BossOverheatState(BossController controller, BossStateMachine stateMachine, BossConfig config) : base(controller, stateMachine, config) { }

        public override void Enter()
        {
            base.Enter();
            controller.SetMovementEnabled(false);
            controller.PlayAnimation("Idle");

            if (controller.IsEnraged)
            {
                _cooldownDuration = config.EnragedOverheatDuration;
            }
            else
            {
                _cooldownDuration = Random.Range(config.OverheatDurationRange.x, config.OverheatDurationRange.y);
            }
        }

        public override void LogicUpdate()
        {
            if (Time.time >= startTime + _cooldownDuration)
            {
                stateMachine.ChangeState(controller.ChaseState);
            }
        }
    }
}
