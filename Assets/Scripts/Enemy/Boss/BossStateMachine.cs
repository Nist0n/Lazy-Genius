using UnityEngine;

namespace Enemy.Boss
{
    public abstract class BossState
    {
        protected readonly BossController controller;
        protected readonly BossStateMachine stateMachine;
        protected readonly BossConfig config;
        protected float startTime;
        protected bool isExitingState;

        protected BossState(BossController controller, BossStateMachine stateMachine, BossConfig config)
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

        public virtual void LogicUpdate()
        {
        }
    }

    public class BossStateMachine
    {
        public BossState CurrentState { get; private set; }
        public BossState PreviousState { get; private set; }

        public void Initialize(BossState startingState)
        {
            CurrentState = startingState;
            CurrentState.Enter();
        }

        public void ChangeState(BossState newState)
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
}
