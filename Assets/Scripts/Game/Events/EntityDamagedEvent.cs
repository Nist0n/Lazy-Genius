using UnityEngine;

namespace Game.Events
{
    public readonly struct EntityDamagedEvent
    {
        public readonly GameObject Target;
        public readonly float Damage;

        public EntityDamagedEvent(GameObject target, float damage)
        {
            Target = target;
            Damage = damage;
        }
    }
}
