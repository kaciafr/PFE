using PnjDetection;
using UnityEngine;
using UnityEngine.AI;
using Utilities;

public class PnjMove : PnjAptitude
{
    [SerializeField] private NavMeshAgent agent;

    public void Update()
    {
	    Vector3 destinationv= Direction.GetDirection(agent,);
    }
}
