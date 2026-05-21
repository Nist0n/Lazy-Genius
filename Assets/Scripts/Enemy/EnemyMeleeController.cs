using System.Collections;
using Audio;
using Core;
using UnityEngine;

namespace Enemy
{
    public class EnemyMeleeController : EnemyController
    {
        private float _lastMeleeAttackTime;
        private float _meleeAnimTimer;
        private Coroutine _meleeAttackRoutine;

        protected override EnemyFightStateMachine CreateFightStateMachine() =>
            new EnemyMeleeFightStateMachine(this);

        protected override void StopCombatRoutines() => StopMeleeAttackRoutine();

        public override bool ShouldAttackFromChase() => GetDistanceToPlayer() <= GetAttackRange();

        public void BeginMeleeAttack()
        {
            _lastMeleeAttackTime = Time.time - EffectiveAttackCooldown;
            _meleeAnimTimer = 0f;
        }

        public void StopMeleeAttackRoutine()
        {
            if (_meleeAttackRoutine != null)
            {
                StopCoroutine(_meleeAttackRoutine);
                _meleeAttackRoutine = null;
            }
        }

        public void UpdateMeleeAttackFacing()
        {
            if (!PlayerTransform) return;

            Vector3 direction = (PlayerTransform.position - transform.position).normalized;
            direction.y = 0;
            if (direction == Vector3.zero) return;

            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
        }

        public bool TryStartMeleeAttackCycle()
        {
            if (_meleeAttackRoutine != null)
            {
                return false;
            }

            if (Time.time < _lastMeleeAttackTime + EffectiveAttackCooldown)
            {
                return false;
            }

            if (Anim) Anim.Play(AttackAnimState);
            _lastMeleeAttackTime = Time.time;
            StopMeleeAttackRoutine();
            _meleeAttackRoutine = StartCoroutine(MeleeAttackRoutine());
            return true;
        }

        private IEnumerator MeleeAttackRoutine()
        {
            _meleeAnimTimer = 0f;

            while (_meleeAnimTimer <= 0.5f)
            {
                _meleeAnimTimer += Time.deltaTime;
                yield return null;
            }

            PerformMeleeHit();

            while (_meleeAnimTimer <= 2f)
            {
                _meleeAnimTimer += Time.deltaTime;
                yield return null;
            }

            _meleeAttackRoutine = null;

            if (GetDistanceToPlayer() > GetAttackRange())
            {
                FightStateMachine.ChangeState(FightStateMachine.CreateChaseState());
            }
            else
            {
                FightStateMachine.ChangeState(FightStateMachine.CreateCombatState());
            }
        }

        private void PerformMeleeHit()
        {
            if (!PlayerTransform || GetDistanceToPlayer() > GetAttackRange())
            {
                return;
            }

            var damageable = PlayerTransform.GetComponent<IDamageable>();
            AudioManager.Instance.PlayLocalSound("EnemyAttack", SoundSource);
            if (damageable == null) return;

            DamageInfo info = new DamageInfo(
                EffectiveAttackDamage,
                DamageSourceType.Generic,
                gameObject,
                PlayerTransform.position,
                Vector3.zero);

            damageable.TakeDamage(EffectiveAttackDamage, info);
        }
    }
}
