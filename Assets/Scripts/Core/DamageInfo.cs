using UnityEngine;

namespace Core
{
    public enum DamageSourceType
    {
        Generic,
        Ability
    }

    [System.Serializable]
    public struct DamageInfo
    {
        public float Damage;
        public DamageSourceType SourceType;
        public GameObject SourceObject;
        public Vector3 ImpactPoint;
        public Vector3 ImpactNormal;

        public DamageInfo(float damage, DamageSourceType sourceType, GameObject sourceObject, Vector3 impactPoint = default, Vector3 impactNormal = default)
        {
            Damage = damage;
            SourceType = sourceType;
            SourceObject = sourceObject;
            ImpactPoint = impactPoint;
            ImpactNormal = impactNormal;
        }
    }
}
