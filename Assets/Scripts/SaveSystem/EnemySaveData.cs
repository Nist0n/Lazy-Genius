using System;
using UnityEngine;

namespace SaveSystem
{
    [Serializable]
    public class EnemySaveData
    {
        public string key;
        public Vector3 position;
        public Quaternion rotation;
    }
}

