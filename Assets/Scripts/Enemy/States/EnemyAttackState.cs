namespace Enemy.States
{
    public class EnemyAttackState : EnemyFightState
    {
        private EnemyMeleeController Melee => (EnemyMeleeController)StateMachine.Enemy;

        public EnemyAttackState(EnemyFightStateMachine stateMachine) : base(stateMachine) { }

        public override void Enter() => Melee.BeginMeleeAttack();

        public override void Exit() => Melee.StopMeleeAttackRoutine();

        public override void LogicUpdate()
        {
            if (!Melee.PlayerTransform)
            {
                StateMachine.ChangeState(StateMachine.CreateIdleState());
                return;
            }

            Melee.UpdateMeleeAttackFacing();
            Melee.TryStartMeleeAttackCycle();
        }
    }
}
