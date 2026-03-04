namespace Enemy.States
{
    public class EnemyDeathState : EnemyState
    {
        public EnemyDeathState(EnemyController controller, EnemyStateMachine stateMachine, EnemyConfig config) 
            : base(controller, stateMachine, config) { }

        public override void Enter()
        {
            if (controller.Agent) controller.Agent.enabled = false;
            controller.Anim.Play("Death");
        }

        public override void Exit()
        {
            
        }

        public override void LogicUpdate()
        {

        }
    }
}
