namespace Enemy.Boss.States
{
    public class BossRocketBarrageState : BossFightState
    {
        public BossRocketBarrageState(BossFightStateMachine stateMachine) : base(stateMachine) { }

        public override void Enter() => StateMachine.Boss.EnterRocketBarrage();

        public override void Exit() => StateMachine.Boss.ExitRocketBarrage();
    }
}
