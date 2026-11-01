using System.Linq;
using UnityEngine;

namespace Goblfin
{
    public class ShadowCast : MonoBehaviour
    {
        [SerializeField] private Transform shadowTransform;
        [SerializeField] private Transform lightTransform;
        private LightType lightType;
        
        [SerializeField] private LayerMask targetMask;
        [SerializeField] private Vector3 extrusionDirection;
        
        private Vector3[] objectVertices;
        private Mesh shadowMesh;
        private MeshCollider  shadowCollider;
        
        private Vector3 previousPosition;
        private Quaternion previousRotation;
        private Vector3 previousScale;
        private bool canUpdateCollider = true;
        
        [SerializeField][Range(0.02f,1f)]  private float updateInterval;
        

        private void Awake()
        {
            InitShadowCollider();
            lightType = LightType.Directional;
            objectVertices = transform.GetComponent<MeshFilter>().mesh.vertices.Distinct().ToArray();
            shadowMesh = new Mesh();
            
        }

        private void Update()
        {
           shadowTransform.position = transform.position;
        }

        private void FixedUpdate()
        {
            if (TransformListChanged() && canUpdateCollider)
            {
                UpdateShadowcollider();
                canUpdateCollider = false;
            }
            
            previousPosition = transform.position;
            previousRotation = transform.rotation;
            previousScale = transform.localScale;
        }
        private void UpdateShadowcollider()
        {
            shadowMesh.vertices = ComputeShadowMeshVertices();
            shadowCollider.sharedMesh = shadowMesh;
            canUpdateCollider =  true;
        }

        private void InitShadowCollider()
        {
            GameObject shadowGo = shadowTransform.gameObject;
            shadowCollider = shadowGo.GetComponent<MeshCollider>();
            shadowCollider.convex = true;
            shadowCollider.isTrigger = true;
        }

        private Vector3[] ComputeShadowMeshVertices()
        {
            Vector3[] points = new Vector3[2 * objectVertices.Length];

            Vector3 raycastDirection = lightTransform.forward;
            
            int n = objectVertices.Length;

            for (int i = 0; i < n; i++)
            {
                Vector3  point = transform.TransformPoint(objectVertices[i]);

                if (lightType != LightType.Directional)
                {
                    raycastDirection = point - lightTransform.position;
                }
                
                points[i] = ComputeInteractPoint(point,raycastDirection);
                points[n + i] = ComputerExtrusionPoint(point, points[i]);
            }
            return points;
        }

        private Vector3 ComputeInteractPoint(Vector3 fromPosition, Vector3 direction)
        {
            RaycastHit hit;
            if (Physics.Raycast(fromPosition, direction, out hit, Mathf.Infinity, targetMask))
            {
                return shadowTransform.InverseTransformPoint(hit.point);
            }
    
            Vector3 fallbackPoint = fromPosition + (direction.normalized * 100f);
            return shadowTransform.InverseTransformPoint(fallbackPoint);
        }

        private Vector3 ComputerExtrusionPoint(Vector3 objectVertexPosition, Vector3 shadowPointPosition)
        {
            if (extrusionDirection.sqrMagnitude == 0)
            {
                return shadowTransform.InverseTransformPoint(objectVertexPosition);
            }
    
            Vector3 worldExtruded = shadowTransform.TransformPoint(shadowPointPosition) + extrusionDirection;
            return shadowTransform.InverseTransformPoint(worldExtruded);
        }

        private bool TransformListChanged()
        {
            return previousPosition != transform.position || previousRotation != transform.rotation || previousScale != transform.localScale;
        }
    }
}
