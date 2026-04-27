using Enemy.States;

namespace Enemy
{
    public class EnemyRangedController : EnemyController
    {
        protected override void CreateCombatStates()
        {
            AttackState = null;
            RangedCombatState = new EnemyRangedCombatState(this, StateMachine, EnemyConfig);
        }

        public override EnemyState GetInitialEngageState()
        {
            if (IsPeacefulModeEnabled)
            {
                if (ShouldAvoidByLowHealth) return AvoidState;
            }

            return RangedCombatState;
        }

        public override EnemyState GetPostHitState(float distanceToPlayer)
        {
            if (IsPeacefulModeEnabled)
            {
                if (ShouldAvoidByLowHealth) return AvoidState;
                
                return IdleState;
            }

            return RangedCombatState;
        }
    }
}
