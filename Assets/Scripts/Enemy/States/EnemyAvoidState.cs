using UnityEngine;

namespace Enemy.States
{
    public class EnemyAvoidState : EnemyState
    {
        public EnemyAvoidState(EnemyController controller, EnemyStateMachine stateMachine, EnemyConfig config) 
            : base(controller, stateMachine, config) { }

        public override void Enter()
        {
            if (controller.Agent) controller.Agent.enabled = true;
            if (controller.Agent.isOnNavMesh) controller.Agent.isStopped = false;
        }

        public override void Exit()
        {
            controller.Agent.isStopped = false;
        }

        public override void LogicUpdate()
        {
            if (!controller.PlayerTransform)
            {
                stateMachine.ChangeState(controller.IdleState);
                return;
            }

            if (!controller.IsPeacefulModeEnabled || !controller.ShouldAvoidByLowHealth)
            {
                stateMachine.ChangeState(controller.IdleState);
                return;
            }
            
            Vector3 toPlayer = controller.PlayerTransform.position - controller.transform.position;
            Vector3 flatDir = new Vector3(toPlayer.x, 0f, toPlayer.z);
            if (flatDir.sqrMagnitude > 0.001f)
            {
                Quaternion lookRotation = Quaternion.LookRotation(flatDir.normalized);
                controller.transform.rotation = Quaternion.Slerp(controller.transform.rotation, lookRotation, Time.deltaTime * 5f);
            }
            
            float distance = flatDir.magnitude;
            
            if (!controller.Agent || !controller.Agent.isOnNavMesh)
            {
                return;
            }
            
            float min = Mathf.Max(0.1f, config.PreferredMinDistance);
            float max = Mathf.Max(min + 0.1f, config.PreferredMaxDistance);
            float runAway = Mathf.Max(0.1f, config.RunAwayDistance);
            
            Vector3 enemyPos = controller.transform.position;
            Vector3 playerPos = controller.PlayerTransform.position;
            Vector3 dirToPlayer = (playerPos - enemyPos).normalized;
            Vector3 dirAwayFromPlayer = -dirToPlayer;

            Vector3 targetPos = enemyPos;
            
            float desiredDistance = min + (max - min) * 0.5f;
            
            if (distance >= desiredDistance && distance >= runAway)
            {
                controller.Agent.isStopped = true;
                controller.Anim.Play("Idle");
                return;
            }

            float moveDistance = Mathf.Clamp(desiredDistance - distance, 2f, desiredDistance);
            targetPos = enemyPos + dirAwayFromPlayer * moveDistance;
            
            controller.Anim.Play("Chase");

            controller.Agent.isStopped = false;
            controller.Agent.SetDestination(targetPos);
        }
    }
}
