using System.Collections.Generic;
using UnityEngine;

namespace Goblfin
{
    public abstract class OverlapTrigger<T> : MonoBehaviour where T : class
    {
        public enum ZoneShape { Box, Sphere }

        [Header("Zone")]
        [SerializeField] protected ZoneShape shape = ZoneShape.Box;
        [SerializeField] protected Vector3 center = Vector3.zero;
        [SerializeField] protected Vector3 size = Vector3.one;      // Box
        [SerializeField] protected float radius = 1f;               // Sphere

        [Header("Détection")]
        [SerializeField] protected LayerMask targetLayers = ~0;
        [SerializeField] protected QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.Ignore;
        [SerializeField, Min(0f)] protected float checkInterval = 0.1f;
        [SerializeField, Min(1)] protected int maxTargets = 16;

        private Collider[] buffer;
        private HashSet<T> previous = new HashSet<T>();
        private HashSet<T> current = new HashSet<T>();
        private float nextCheck;

        protected virtual bool ShouldCheck => true;

        protected virtual void Awake() => buffer = new Collider[maxTargets];

        protected virtual void OnDisable()
        {
            foreach (var target in previous)
                if (!IsDestroyed(target)) OnZoneExit(target);
            previous.Clear();
        }

        protected virtual void Update()
        {
            if (Time.time < nextCheck || !ShouldCheck) return;
            nextCheck = Time.time + checkInterval;

            int count = Query();

            current.Clear();
            for (int i = 0; i < count; i++)
            {
                T target = ResolveTarget(buffer[i]);
                if (target == null || !current.Add(target)) continue;

                if (previous.Contains(target)) OnZoneStay(target);
                else OnZoneEnter(target);
            }

            foreach (var target in previous)
                if (!current.Contains(target) && !IsDestroyed(target))
                    OnZoneExit(target);

            (previous, current) = (current, previous);
        }

        // Un objet Unity détruit n'est pas "== null" quand il est vu à travers une interface
        private static bool IsDestroyed(T target) =>
            target == null || (target is UnityEngine.Object unityObject && unityObject == null);

        private int Query()
        {
            Vector3 worldCenter = transform.TransformPoint(center);
            Vector3 lossy = transform.lossyScale;
            Vector3 absScale = new Vector3(Mathf.Abs(lossy.x), Mathf.Abs(lossy.y), Mathf.Abs(lossy.z));

            if (shape == ZoneShape.Box)
            {
                return Physics.OverlapBoxNonAlloc(worldCenter, Vector3.Scale(size, absScale) * 0.5f,
                    buffer, transform.rotation, targetLayers, triggerInteraction);
            }

            float maxScale = Mathf.Max(absScale.x, Mathf.Max(absScale.y, absScale.z));
            return Physics.OverlapSphereNonAlloc(worldCenter, radius * maxScale,
                buffer, targetLayers, triggerInteraction);
        }

        protected virtual T ResolveTarget(Collider col) => col.GetComponentInParent<T>();

        protected virtual void OnZoneEnter(T target) { }
        protected virtual void OnZoneStay(T target) { }
        protected virtual void OnZoneExit(T target) { }

#if UNITY_EDITOR
        protected virtual Color GizmoColor => Color.green;

        private void OnDrawGizmos()
        {
            Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, transform.lossyScale);

            Color c = GizmoColor;
            Gizmos.color = new Color(c.r, c.g, c.b, 0.2f);
            if (shape == ZoneShape.Box) Gizmos.DrawCube(center, size);
            else Gizmos.DrawSphere(center, radius);

            Gizmos.color = c;
            if (shape == ZoneShape.Box) Gizmos.DrawWireCube(center, size);
            else Gizmos.DrawWireSphere(center, radius);
        }
#endif
    }
}