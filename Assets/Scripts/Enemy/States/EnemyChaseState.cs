namespace Enemy.States
{
    public class EnemyChaseState : EnemyFightState
    {
        public EnemyChaseState(EnemyFightStateMachine stateMachine) : base(stateMachine) { }

        public override void Enter() => StateMachine.Enemy.EnterChase();

        public override void Exit() => StateMachine.Enemy.ExitChase();

        public override void LogicUpdate()
        {
            if (!StateMachine.Enemy.PlayerTransform)
            {
                return;
            }

            StateMachine.Enemy.UpdateChaseMovement();

            if (StateMachine.Enemy.ShouldAttackFromChase())
            {
                StateMachine.ChangeState(StateMachine.CreateCombatState());
            }
            else if (StateMachine.Enemy.ShouldLosePlayerFromChase())
            {
                StateMachine.ChangeState(StateMachine.CreateIdleState());
            }
        }
    }
}
