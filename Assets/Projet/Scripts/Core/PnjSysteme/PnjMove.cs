using System;
using PnjDetection;
using UnityEngine;
using UnityEngine.AI;
using Utilities;

public class PnjMove : PnjAptitude
{
    [SerializeField] private NavMeshAgent agent;
    private Vector3 lastPos;

    public override void Init(BrainPnj brain)
    {
	    base.Init(brain);

	    VisionCone vision = brain.GetAptitude<VisionCone>();
	    if (vision != null)
		    vision.OnTargetSeen += HandleTargetSeen;
    }
    
    void OnDestroy()
    {
	    if (brain == null) return;
	    VisionCone vision = brain.GetAptitude<VisionCone>();
	    if (vision != null)
		    vision.OnTargetSeen -= HandleTargetSeen;
    }

    private void HandleTargetSeen(Vector3 obj)
    {
	    lastPos = obj;
    }

    private void Start()
    {
	    lastPos = agent.transform.position;
    }

    private void Update()
    {
	    agent.SetDestination(lastPos);
    }
}
