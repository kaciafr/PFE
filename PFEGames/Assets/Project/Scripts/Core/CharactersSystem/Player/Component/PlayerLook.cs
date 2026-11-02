using Goblfin.CharactersSystem.Player.Component;
using UnityEngine;
using UnityEngine.Animations.Rigging;

namespace Goblfin
{
    public class PlayerLook : MonoBehaviour
    {
        [SerializeField] private Rig lookRig;
        [SerializeField] private Transform lookTarget;
        [SerializeField] private float detectRadius = 4f;
        [SerializeField] private float targetSpeed = 6f;  
        [SerializeField] private float weightSpeed = 1f;
        [SerializeField] private float maxAngle = 110f;
        
        private Transform head;
        private LookInterest interest;

        private void Awake()
        {
            head = GetComponent<Animator>().GetBoneTransform(HumanBodyBones.Head);
        }

        void Start()
        {
        
        }

        private void Update()
        {
            interest = FindInterest();
            
            Vector3 wanted = interest != null ? interest.LookPosition : head.position + transform.forward * 2f; 
            
            lookTarget.position = Vector3.Lerp(lookTarget.position, wanted, targetSpeed * Time.deltaTime);
            
            float wantedHeight = interest != null ? 1f: 0f;
            
            lookRig.weight = Mathf.MoveTowards(lookRig.weight, wantedHeight, weightSpeed * Time.deltaTime);
            
        }

        private LookInterest FindInterest()
        {
            LookInterest best = null;
            float bestDistance =  Mathf.Infinity;

            foreach ( Collider  hit  in  Physics.OverlapSphere(head.position, detectRadius))
            {
                LookInterest candidate = hit.GetComponentInParent<LookInterest>();
                if (candidate == null) continue;

                Vector3 direction = candidate.LookPosition - head.position;
                direction.y = 0f;
                if(Vector3.Angle(transform.forward , direction ) > maxAngle) continue;

                float distance = direction.magnitude;
                bool better = best == null
                    || candidate.Priority > best.Priority
                    || (candidate.Priority == best.Priority && distance < bestDistance);

                if (better)
                {
                    bestDistance = distance;
                    best = candidate;
                }
            }
            return best;
        }
        
        private void OnDrawGizmosSelected()
        {
            if (head != null) Gizmos.DrawWireSphere(head.position, detectRadius);
        }
    }
}
