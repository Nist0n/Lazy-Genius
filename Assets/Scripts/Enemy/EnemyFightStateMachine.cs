using Enemy.States;

namespace Enemy
{
    public abstract class EnemyFightState
    {
        protected readonly EnemyFightStateMachine StateMachine;

        protected EnemyFightState(EnemyFightStateMachine stateMachine)
        {
            StateMachine = stateMachine;
        }

        public virtual void Enter() { }
        public virtual void Exit() { }
        public virtual void LogicUpdate() { }
        public virtual void PhysicsUpdate() { }
        public virtual void OnCollisionEnter(UnityEngine.Collision collision) { }
    }

    public class EnemyFightStateMachine
    {
        public EnemyFightState CurrentState { get; private set; }
        public EnemyFightState PreviousState { get; private set; }
        public EnemyController Enemy { get; }

        public EnemyFightStateMachine(EnemyController enemy)
        {
            Enemy = enemy;
        }

        public virtual void Initialize()
        {
            ChangeState(CreateIdleState());
        }

        public virtual EnemyFightState CreateIdleState() => new EnemyIdleState(this);
        public virtual EnemyFightState CreateChaseState() => new EnemyChaseState(this);
        public virtual EnemyFightState CreateGetHitState() => new EnemyGetHitState(this);
        public virtual EnemyFightState CreateDeathState() => new EnemyDeathState(this);
        public virtual EnemyFightState CreateAvoidState() => new EnemyAvoidState(this);
        public virtual EnemyFightState CreateCombatState() => new EnemyAttackState(this);
        public virtual EnemyFightState CreateEngageState() => CreateChaseState();

        public void ChangeState(EnemyFightState newState)
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

    public class EnemyMeleeFightStateMachine : EnemyFightStateMachine
    {
        public EnemyMeleeFightStateMachine(EnemyMeleeController enemy) : base(enemy) { }
    }

    public class EnemyRangedFightStateMachine : EnemyFightStateMachine
    {
        public EnemyRangedFightStateMachine(EnemyRangedController enemy) : base(enemy) { }

        public override EnemyFightState CreateCombatState() => new EnemyRangedCombatState(this);

        public override EnemyFightState CreateEngageState() => CreateCombatState();
    }
}
