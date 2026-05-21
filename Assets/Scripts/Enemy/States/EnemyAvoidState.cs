namespace Enemy.States
{
    public class EnemyAvoidState : EnemyFightState
    {
        public EnemyAvoidState(EnemyFightStateMachine stateMachine) : base(stateMachine) { }

        public override void Enter() => StateMachine.Enemy.EnterAvoid();

        public override void Exit() => StateMachine.Enemy.ExitAvoid();

        public override void LogicUpdate()
        {
            if (!StateMachine.Enemy.PlayerTransform)
            {
                StateMachine.ChangeState(StateMachine.CreateIdleState());
                return;
            }

            if (StateMachine.Enemy.ShouldStopAvoiding())
            {
                StateMachine.ChangeState(StateMachine.CreateIdleState());
                return;
            }

            StateMachine.Enemy.UpdateAvoidMovement();
        }
    }
}
