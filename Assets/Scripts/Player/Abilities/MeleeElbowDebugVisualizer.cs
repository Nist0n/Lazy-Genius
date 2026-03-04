using UnityEngine;

namespace Player.Abilities
{
    public class MeleeElbowDebugVisualizer : MonoBehaviour
    {
        [SerializeField] private Color gizmoColor = Color.red;

        private Vector3 lastOriginPosition;
        private Vector3 lastHitCenter;
        private float lastHitRadius;
        private bool hasData;

        public void SetHitData(Vector3 originPosition, Vector3 hitCenter, float hitRadius)
        {
            lastOriginPosition = originPosition;
            lastHitCenter = hitCenter;
            lastHitRadius = hitRadius;
            hasData = true;
        }

        private void OnDrawGizmos()
        {
            if (!hasData)
                return;

            Gizmos.color = gizmoColor;

            Gizmos.DrawLine(lastOriginPosition, lastHitCenter);
            Gizmos.DrawWireSphere(lastHitCenter, lastHitRadius);
        }
    }
}

