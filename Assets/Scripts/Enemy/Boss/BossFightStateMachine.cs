using Enemy.Boss.States;

namespace Enemy.Boss
{
    public abstract class BossFightState
    {
        protected readonly BossFightStateMachine StateMachine;
        protected float StartTime;
        protected bool IsExitingState;

        protected BossFightState(BossFightStateMachine stateMachine)
        {
            StateMachine = stateMachine;
        }

        public virtual void Enter()
        {
            IsExitingState = false;
            StartTime = UnityEngine.Time.time;
        }

        public virtual void Exit()
        {
            IsExitingState = true;
        }

        public virtual void LogicUpdate() { }
    }

    public class BossFightStateMachine
    {
        public BossFightState CurrentState { get; private set; }
        public BossFightState PreviousState { get; private set; }
        public BossController Boss { get; }
        public float SpeedFactor { get; protected set; } = 1f;

        public BossFightStateMachine(BossController boss)
        {
            Boss = boss;
        }

        public virtual void Initialize()
        {
            ChangeState(CreateIdleState());
        }

        public virtual BossFightState CreateIdleState() => new BossIdleState(this);
        public virtual BossFightState CreateChaseState() => new BossChaseState(this);
        public virtual BossFightState CreateBasicAttackState() => new BossBasicRangedAttackState(this);
        public virtual BossFightState CreateSuppressiveFireState() => new BossSuppressiveFireState(this);
        public virtual BossFightState CreateRocketBarrageState() => new BossRocketBarrageState(this);
        public virtual BossFightState CreateOverheatState() => new BossOverheatState(this);
        public virtual BossFightState CreateDeathState() => new BossDeathState(this);
        public virtual BossFightState CreateEnragedIntroState() => new BossEnragedIntroState(this);

        public BossFightState PickChaseCombatState()
        {
            float roll = UnityEngine.Random.value;
            if (Boss.IsRocketReady() && roll < 0.3f)
            {
                return CreateRocketBarrageState();
            }

            if (roll < 0.65f)
            {
                return CreateBasicAttackState();
            }

            return CreateSuppressiveFireState();
        }

        public void ChangeState(BossFightState newState)
        {
            if (newState == null || CurrentState == newState)
            {
                return;
            }

            PreviousState = CurrentState;
            CurrentState?.Exit();
            CurrentState = newState;
            CurrentState.Enter();
        }
    }

    public class BossEnragedFightStateMachine : BossFightStateMachine
    {
        public BossEnragedFightStateMachine(BossController boss) : base(boss)
        {
            if (boss.Config) SpeedFactor = boss.Config.EnragedMoveSpeedMultiplier;
            else SpeedFactor = 1.4f;
        }

        public override void Initialize()
        {
            Boss.BeginEnragedPhase();
            ChangeState(CreateEnragedIntroState());
        }
    }
}
