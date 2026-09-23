using UnityEngine;
using UnityEngine.AI;

public class TestRagDoll : MonoBehaviour
{
    public NavMeshAgent brainPnj;
    public Transform targetPosition;
    public Animator animator;

    public void Update()
    {
	    animator.SetFloat("Speed", brainPnj.velocity.magnitude);
	    brainPnj.SetDestination(targetPosition.position);
	    
    }
}
