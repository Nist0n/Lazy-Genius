namespace Enemy.Boss.States
{
    public class BossOverheatState : BossFightState
    {
        private float _cooldownDuration;

        public BossOverheatState(BossFightStateMachine stateMachine) : base(stateMachine) { }

        public override void Enter()
        {
            base.Enter();
            StateMachine.Boss.EnterOverheat();
            _cooldownDuration = StateMachine.Boss.GetOverheatDuration();
        }

        public override void LogicUpdate()
        {
            if (StateMachine.Boss.IsOverheatComplete(StartTime, _cooldownDuration))
            {
                StateMachine.ChangeState(StateMachine.CreateChaseState());
            }
        }
    }
}
