namespace Enemy.Boss.States
{
    public class BossEnragedIntroState : BossFightState
    {
        public BossEnragedIntroState(BossFightStateMachine stateMachine) : base(stateMachine) { }

        public override void Enter() => StateMachine.Boss.EnterEnragedIntro();

        public override void LogicUpdate()
        {
            if (StateMachine.Boss.IsEnragedIntroComplete())
            {
                StateMachine.ChangeState(StateMachine.CreateChaseState());
            }
        }
    }
}
