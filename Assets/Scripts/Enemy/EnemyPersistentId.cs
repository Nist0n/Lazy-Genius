using System;
using UnityEngine;

namespace Enemy
{
    public sealed class EnemyPersistentId : MonoBehaviour
    {
        [SerializeField] private string id;
        public string Id => id;

        private void OnValidate()
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                id = Guid.NewGuid().ToString();
            }
        }
    }
}

