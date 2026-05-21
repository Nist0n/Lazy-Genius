namespace Enemy.Boss.States
{
    public class BossBasicRangedAttackState : BossFightState
    {
        public BossBasicRangedAttackState(BossFightStateMachine stateMachine) : base(stateMachine) { }

        public override void Enter() => StateMachine.Boss.EnterBasicAttack();

        public override void Exit() => StateMachine.Boss.ExitBasicAttack();

        public override void LogicUpdate() => StateMachine.Boss.UpdateBasicAttackFacing();
    }
}
