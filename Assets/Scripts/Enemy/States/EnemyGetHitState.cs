namespace Enemy.States
{
    public class EnemyGetHitState : EnemyFightState
    {
        public EnemyGetHitState(EnemyFightStateMachine stateMachine) : base(stateMachine) { }

        public override void Enter() => StateMachine.Enemy.EnterGetHit();

        public override void LogicUpdate()
        {
            if (!StateMachine.Enemy.IsGetHitRecoveryComplete())
            {
                return;
            }

            if (!StateMachine.Enemy.PlayerTransform)
            {
                StateMachine.ChangeState(StateMachine.CreateIdleState());
                return;
            }

            if (StateMachine.Enemy.ShouldFleeAfterHit() || StateMachine.Enemy.ShouldEnterAvoidFromIdle())
            {
                StateMachine.ChangeState(StateMachine.CreateAvoidState());
            }
            else if (StateMachine.Enemy.ShouldReturnIdleAfterHit())
            {
                StateMachine.ChangeState(StateMachine.CreateIdleState());
            }
            else if (StateMachine.Enemy.ShouldChaseAfterHit())
            {
                StateMachine.ChangeState(StateMachine.CreateChaseState());
            }
            else if (StateMachine.Enemy.ShouldCombatAfterHit())
            {
                StateMachine.ChangeState(StateMachine.CreateCombatState());
            }
            else
            {
                StateMachine.ChangeState(StateMachine.CreateIdleState());
            }
        }
    }
}
