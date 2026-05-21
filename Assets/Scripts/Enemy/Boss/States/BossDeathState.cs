namespace Enemy.Boss.States
{
    public class BossDeathState : BossFightState
    {
        public BossDeathState(BossFightStateMachine stateMachine) : base(stateMachine) { }

        public override void Enter() => StateMachine.Boss.EnterDeath();
    }
}
