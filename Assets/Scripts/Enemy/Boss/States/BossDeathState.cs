namespace Enemy.Boss.States
{
    public class BossDeathState : BossState
    {
        public BossDeathState(BossController controller, BossStateMachine stateMachine, BossConfig config) : base(controller, stateMachine, config)
        {
        }

        public override void Enter()
        {
            controller.SetMovementEnabled(false);
            if (controller.Agent)
            {
                controller.Agent.enabled = false;
            }

            controller.PlayAnimation("Death");
            controller.StartSelfDestruct();
        }

        public override void LogicUpdate()
        {
        }
    }
}
