using UnityEngine;

namespace Game.Events
{
    public readonly struct MobKilledEvent
    {
        public readonly GameObject Mob;

        public MobKilledEvent(GameObject mob)
        {
            Mob = mob;
        }
    }
}
