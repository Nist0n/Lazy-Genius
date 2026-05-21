namespace Enemy.States
{
    public class EnemyDeathState : EnemyFightState
    {
        public EnemyDeathState(EnemyFightStateMachine stateMachine) : base(stateMachine) { }

        public override void Enter() => StateMachine.Enemy.EnterDeath();
    }
}
