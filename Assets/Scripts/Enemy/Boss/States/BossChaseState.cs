namespace Enemy.Boss.States
{
    public class BossChaseState : BossFightState
    {
        public BossChaseState(BossFightStateMachine stateMachine) : base(stateMachine) { }

        public override void Enter() => StateMachine.Boss.EnterChase();

        public override void Exit() => StateMachine.Boss.ExitChase();

        public override void LogicUpdate()
        {
            if (!StateMachine.Boss.PlayerTransform)
            {
                StateMachine.ChangeState(StateMachine.CreateIdleState());
                return;
            }

            StateMachine.Boss.UpdateChase();

            if (StateMachine.Boss.ShouldReturnIdleFromChase())
            {
                StateMachine.ChangeState(StateMachine.CreateIdleState());
                return;
            }

            if (!StateMachine.Boss.CanAttackFromChase())
            {
                return;
            }

            StateMachine.ChangeState(StateMachine.PickChaseCombatState());
        }
    }
}
