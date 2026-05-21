namespace Enemy.Boss.States
{
    public class BossSuppressiveFireState : BossFightState
    {
        public BossSuppressiveFireState(BossFightStateMachine stateMachine) : base(stateMachine) { }

        public override void Enter() => StateMachine.Boss.EnterSuppressiveFire();

        public override void Exit() => StateMachine.Boss.ExitSuppressiveFire();
    }
}
