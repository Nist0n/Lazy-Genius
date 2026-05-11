using System;
using UnityEngine;

namespace Enemy
{
    public sealed class EnemyPersistentId : MonoBehaviour
    {
        [SerializeField] private string id;
        public string Id => id;

        public void SetId(string newId)
        {
            id = string.IsNullOrWhiteSpace(newId) ? Guid.NewGuid().ToString("N") : newId;
        }

        private void OnValidate()
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                id = Guid.NewGuid().ToString();
            }
        }
    }
}

