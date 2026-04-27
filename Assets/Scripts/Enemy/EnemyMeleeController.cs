namespace Enemy
{
    public class EnemyMeleeController : EnemyController
    {
        public override EnemyState GetInitialEngageState()
        {
            if (IsPeacefulModeEnabled)
            {
                if (ShouldAvoidByLowHealth) return AvoidState;
            }

            return ChaseState;
        }

        public override EnemyState GetPostHitState(float distanceToPlayer)
        {
            if (IsPeacefulModeEnabled)
            {
                if (ShouldAvoidByLowHealth) return AvoidState;
                
                return IdleState;
            }

            if (distanceToPlayer > EnemyConfig.AttackRange) return ChaseState;
            
            return AttackState;
        }
    }
}
