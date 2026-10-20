using UnityEngine;

namespace Goblfin.CharactersSystem.Player.Component
{
    public class SnapPoint : MonoBehaviour
    {
        public static SnapPoint FindForFace(UnityEngine.Component target, Vector3 hitPoint, Vector3 hitNormal)
        {
            hitNormal.y = 0f;
            hitNormal.Normalize();

            SnapPoint best = null;
            float bestDistance = float.MaxValue;

            foreach (SnapPoint point in target.GetComponentsInChildren<SnapPoint>())
            {
                Vector3 pointForward = point.transform.forward;
                pointForward.y = 0f;
                if (Vector3.Dot(pointForward.normalized, -hitNormal) < 0.7f) continue;

                float distance = (point.transform.position - hitPoint).sqrMagnitude;
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    best = point;
                }
            }

            return best;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, 0.15f);
            Gizmos.DrawLine(transform.position, transform.position + transform.forward * 0.5f);
        }
    }
}
