using UnityEngine;

namespace Enemy
{
    public abstract class EnemyState
    {
        protected EnemyController controller;
        protected EnemyStateMachine stateMachine;
        protected EnemyConfig config;
        protected bool isExitingState;
        protected float startTime;

        public EnemyState(EnemyController controller, EnemyStateMachine stateMachine, EnemyConfig config)
        {
            this.controller = controller;
            this.stateMachine = stateMachine;
            this.config = config;
        }

        public virtual void Enter()
        {
            isExitingState = false;
            startTime = Time.time;
        }

        public virtual void Exit()
        {
            isExitingState = true;
        }

        public virtual void LogicUpdate() { }
        public virtual void PhysicsUpdate() { }
        
        public virtual void OnCollisionEnter(Collision collision) { }
    }

    public class EnemyStateMachine
    {
        public EnemyState CurrentState { get; private set; }
        public EnemyState PreviousState { get; private set; }

        public void Initialize(EnemyState startingState)
        {
            CurrentState = startingState;
            CurrentState.Enter();
        }

        public void ChangeState(EnemyState newState)
        {
            if (CurrentState == newState) return;

            PreviousState = CurrentState;
            CurrentState.Exit();
            CurrentState = newState;
            CurrentState.Enter();
        }
    }
}
