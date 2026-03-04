using Audio;
using UnityEngine;

namespace Enemy.States
{
    public class EnemyChaseState : EnemyState
    {
        public EnemyChaseState(EnemyController controller, EnemyStateMachine stateMachine, EnemyConfig config) 
            : base(controller, stateMachine, config) { }

        public override void Enter()
        {
            controller.StepsSource.enabled = true;
            if (controller.Agent) controller.Agent.enabled = true; // Ensure agent is enabled
            if (controller.Agent.isOnNavMesh) controller.Agent.isStopped = false;
            controller.Anim.Play("Chase");
        }

        public override void Exit()
        {
            controller.StepsSource.enabled = false;
            controller.Agent.isStopped = true;
        }

        public override void LogicUpdate()
        {
            if (!controller.PlayerTransform) return;

            controller.Agent.SetDestination(controller.PlayerTransform.position);

            float distance = Vector3.Distance(controller.transform.position, controller.PlayerTransform.position);

            if (distance <= config.AttackRange)
            {
                stateMachine.ChangeState(controller.AttackState);
            }
            else if (distance > config.DetectionRadius * 1.5f)
            {
                stateMachine.ChangeState(controller.IdleState);
            }
        }
    }
}
