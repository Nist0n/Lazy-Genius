using System;
using System.Collections;
using System.Threading.Tasks;
using Audio;
using Core;
using UnityEngine;

namespace Enemy.States
{
    public class EnemyAttackState : EnemyState
    {
        private float _lastAttackTime;
        private float _animTimer;

        public EnemyAttackState(EnemyController controller, EnemyStateMachine stateMachine, EnemyConfig config) 
            : base(controller, stateMachine, config) { }

        public override void Enter()
        {
            _lastAttackTime = Time.time - config.AttackCooldown;
            _animTimer = 0;
        }

        public override void LogicUpdate()
        {
            if (!controller.PlayerTransform)
            {
                stateMachine.ChangeState(controller.IdleState);
                return;
            }
            
            Vector3 direction = (controller.PlayerTransform.position - controller.transform.position).normalized;
            direction.y = 0;
            if (direction != Vector3.zero)
            {
                Quaternion lookRotation = Quaternion.LookRotation(direction);
                controller.transform.rotation = Quaternion.Slerp(controller.transform.rotation, lookRotation, Time.deltaTime * 5f);
            }

            if (Time.time >= _lastAttackTime + config.AttackCooldown)
            {
                controller.Anim.Play("Attack");
                _lastAttackTime = Time.time;
                StartAttack();
            }
        }

        public void Attack()
        {
            float distance = Vector3.Distance(controller.transform.position, controller.PlayerTransform.position);
            
            if (distance > config.AttackRange)
            {
                return;
            }
            
            var damageable = controller.PlayerTransform.GetComponent<IDamageable>();
            AudioManager.Instance.PlayLocalSound("EnemyAttack", controller.SoundSource);
            if (damageable != null)
            {
                DamageInfo info = new DamageInfo(
                    config.AttackDamage,
                    DamageSourceType.Generic,
                    controller.gameObject,
                    controller.PlayerTransform.position,
                    Vector3.zero
                );
                
                damageable.TakeDamage(config.AttackDamage, info);
            }
        }

        private async void StartAttack()
        {
            try
            {
                while (_animTimer <= 2)
                {
                    _animTimer += Time.deltaTime;
                    await Task.Yield();
                }
                
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
            catch (Exception e)
            {
                Debug.LogWarning(e.Message);
            }
        }
    }
}
