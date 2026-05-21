namespace Enemy.States
{
    public class EnemyRangedCombatState : EnemyFightState
    {
        private EnemyRangedController Ranged => (EnemyRangedController)StateMachine.Enemy;

        public EnemyRangedCombatState(EnemyFightStateMachine stateMachine) : base(stateMachine) { }

        public override void Enter() => Ranged.EnterRangedCombat();

        public override void Exit() => Ranged.ExitRangedCombat();

        public override void LogicUpdate()
        {
            if (!Ranged.PlayerTransform)
            {
                StateMachine.ChangeState(StateMachine.CreateIdleState());
                return;
            }

            Ranged.UpdateRangedCombatFacing();

            if (Ranged.ShouldLosePlayerFromRangedCombat())
            {
                StateMachine.ChangeState(StateMachine.CreateIdleState());
                return;
            }

            Ranged.UpdateRangedCombatMovement();
            Ranged.TryStartRangedShot();
        }
    }
}
