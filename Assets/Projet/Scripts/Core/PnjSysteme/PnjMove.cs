using System;
using PnjDetection;
using UnityEngine;
using UnityEngine.AI;
using Utilities;

public class PnjMove : PnjAptitude
{
    [SerializeField] private NavMeshAgent agent;
    
    private bool isTurning = false;
    private float startToWalk = 5.0f;
    private Vector3 lastPos;
    

    public override void Init(BrainPnj brain)
    {
	    base.Init(brain);

	    VisionCone vision = brain.GetAptitude<VisionCone>();
	    if (vision != null)
		    vision.OnTargetSeen += HandleTargetSeen;
	    
	    AuditionCast audition = brain.GetAptitude<AuditionCast>();
	    if (audition != null)
		    audition.OnHearAlerte += HandleTargetSeen;
    }
    

    private void HandleTargetSeen(Vector3 pos)
    {
	    lastPos = pos;
	    agent.SetDestination(lastPos);
    }

    private void Start()
    {
	    lastPos = agent.transform.position;
    }

    void OnDestroy()
    {
	    if (brain == null) return;
	    VisionCone vision = brain.GetAptitude<VisionCone>();
	    if (vision != null)
		    vision.OnTargetSeen -= HandleTargetSeen;
	    
	    AuditionCast audition = brain.GetAptitude<AuditionCast>();
	    if (audition != null)
		    audition.OnHearAlerte -= HandleTargetSeen;
    }
}
