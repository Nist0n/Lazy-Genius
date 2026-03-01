using UnityEngine;

namespace Enemy.States
{
    public class EnemyGetHitState : EnemyState
    {
        private float _lastHitTime;
        
        public EnemyGetHitState(EnemyController controller, EnemyStateMachine stateMachine, EnemyConfig config) 
            : base(controller, stateMachine, config) { }

        public override void Enter()
        {
            if (controller.Agent) controller.Agent.enabled = false;
            controller.Anim.Play("GetHit");
            _lastHitTime = Time.time;
        }

        public override void LogicUpdate()
        {
            if (Time.time >= _lastHitTime + config.GetHitCooldown)
            {
                float distance = Vector3.Distance(controller.transform.position, controller.PlayerTransform.position);
                
                if (distance > config.AttackRange)
                {
                    stateMachine.ChangeState(controller.ChaseState);
                }
                else
                {
                    stateMachine.ChangeState(controller.AttackState);
                }
            }
        }
    }
}
