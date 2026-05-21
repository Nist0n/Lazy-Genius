namespace Enemy.Boss.States
{
    public class BossIdleState : BossFightState
    {
        public BossIdleState(BossFightStateMachine stateMachine) : base(stateMachine) { }

        public override void Enter() => StateMachine.Boss.EnterIdle();

        public override void LogicUpdate()
        {
            if (StateMachine.Boss.ShouldChaseFromIdle())
            {
                StateMachine.ChangeState(StateMachine.CreateChaseState());
                return;
            }

            if (StateMachine.Boss.ShouldBasicAttackFromIdle())
            {
                StateMachine.ChangeState(StateMachine.CreateBasicAttackState());
            }
        }
    }
}
