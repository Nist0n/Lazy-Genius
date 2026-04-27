using System.Collections;
using UnityEngine;

namespace Enemy.Boss.States
{
    public class BossRocketBarrageState : BossState
    {
        private Coroutine _barrageRoutine;

        public BossRocketBarrageState(BossController controller, BossStateMachine stateMachine, BossConfig config) : base(controller, stateMachine, config)
        {
        }

        public override void Enter()
        {
            base.Enter();
            controller.SetMovementEnabled(false);
            controller.PlayAnimation("Attack");
            _barrageRoutine = controller.StartCoroutine(BarrageRoutine());
        }

        public override void Exit()
        {
            base.Exit();
            if (_barrageRoutine != null)
            {
                controller.StopCoroutine(_barrageRoutine);
                _barrageRoutine = null;
            }
        }

        private IEnumerator BarrageRoutine()
        {
            if (!controller.PlayerTransform)
            {
                stateMachine.ChangeState(controller.IdleState);
                yield break;
            }

            int min = Mathf.Max(1, config.RocketCountRange.x);
            int max = Mathf.Max(min, config.RocketCountRange.y);
            int count = Random.Range(min, max + 1);
            if (controller.IsEnraged)
            {
                count += Mathf.Max(0, config.EnragedExtraRockets);
            }

            Vector3 playerCenter = controller.PlayerTransform.position;
            Vector3[] targets = new Vector3[count];
            for (int i = 0; i < count; i++)
            {
                Vector2 rnd = Random.insideUnitCircle * config.RocketAreaRadius;
                targets[i] = playerCenter + new Vector3(rnd.x, 0f, rnd.y);
                controller.SpawnRocketTelegraph(targets[i]);
            }

            yield return new WaitForSeconds(config.RocketTelegraphDuration);

            for (int i = 0; i < targets.Length; i++)
            {
                controller.FireRocketAt(targets[i]);
                yield return new WaitForSeconds(0.12f);
            }

            controller.LastRocketBarrageTime = Time.time;
            stateMachine.ChangeState(controller.OverheatState);
        }
    }
}
