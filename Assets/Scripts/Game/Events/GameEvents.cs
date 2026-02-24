using System;
using Core;
using UnityEngine;

namespace Game.Events
{
    public static class GameEvents
    {
        public static Action<GameObject, float> EntityDamaged;

        public static void OnEntityDamaged(GameObject target, float damage)
        {
            EntityDamaged?.Invoke(target, damage);
        }
    }
}
