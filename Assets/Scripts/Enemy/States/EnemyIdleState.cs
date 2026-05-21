namespace Enemy.States
{
    public class EnemyIdleState : EnemyFightState
    {
        public EnemyIdleState(EnemyFightStateMachine stateMachine) : base(stateMachine) { }

        public override void Enter() => StateMachine.Enemy.EnterIdle();

        public override void Exit() => StateMachine.Enemy.ExitIdle();

        public override void LogicUpdate()
        {
            if (StateMachine.Enemy.ShouldEnterAvoidFromIdle())
            {
                StateMachine.ChangeState(StateMachine.CreateAvoidState());
                return;
            }

            if (StateMachine.Enemy.ShouldEngageFromIdle())
            {
                StateMachine.ChangeState(StateMachine.CreateEngageState());
            }
        }
    }
}
