using UnityEngine;

namespace Enemy.States
{
    public class EnemyIdleState : EnemyState
    {
        public EnemyIdleState(EnemyController controller, EnemyStateMachine stateMachine, EnemyConfig config) 
            : base(controller, stateMachine, config) { }

        public override void Enter()
        {
            if (controller.Agent) controller.Agent.enabled = true;
            if (controller.Agent.isOnNavMesh) controller.Agent.isStopped = true;
            controller.Anim.Play("Idle");
        }

        public override void Exit()
        {
            controller.Agent.isStopped = false;
        }

        public override void LogicUpdate()
        {
            if (controller.PlayerTransform)
            {
                if (controller.CanSeePlayer())
                {
                    stateMachine.ChangeState(controller.ChaseState);
                }
            }
        }
    }
}
