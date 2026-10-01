using System;
using System.Collections.Generic;
using PnjDetection;
using PnjStates;
using RagdollSysteme;
using Routine;
using UnityEngine;
using UnityEngine.AI;

public class BrainPnj : RagDoll
{
    [SerializeField] private List<PnjAptitude> aptitudes = new List<PnjAptitude>();
    [field:SerializeField] public NavMeshAgent Agent {get; private set;}
    [field:SerializeField] public List<PointTime> FirstRoutine {get; private set;} = new List<PointTime>();
    [field:SerializeField] public int currentStep = 0;
    [field:SerializeField] public Animator Animator {get; private set;}

    public IPnjStates PNJStates { get; private set; }
    public event Action<IPnjStates> OnStatesChanged;

    private Rigidbody[] ragdollRigidbodies;
    private Collider mainCollider;

    private void Awake()
    {
       Animator = GetComponent<Animator>();
       Agent = GetComponent<NavMeshAgent>();

       ragdollRigidbodies = GetComponentsInChildren<Rigidbody>();
       mainCollider = GetComponent<Collider>();

       SetRagdollPhysics(false); // désactivé par défaut au démarrage

       PnjAptitude[] found = GetComponents<PnjAptitude>();
       aptitudes.AddRange(found);

       foreach (PnjAptitude aptitude in aptitudes)
       {
          aptitude.Init(this);
       }
    }

    private void OnEnable()
    {
       VisionCone vision = GetAptitude<VisionCone>();
       if (vision != null)
          vision.OnTargetSeen += HandleTargetSeen;

       AuditionCast audition = GetAptitude<AuditionCast>();
       if (audition != null)
          audition.OnHearAlerte += HandleTargetSound;

       OnRagdoll += EnableRagdollPhysics;
       OnRagdoll += KoMode;
       OnWakeUp += DisableRagdollPhysics;
    }

    private void OnDisable()
    {
       VisionCone vision = GetAptitude<VisionCone>();
       if (vision != null)
          vision.OnTargetSeen -= HandleTargetSeen;

       AuditionCast audition = GetAptitude<AuditionCast>();
       if (audition != null)
          audition.OnHearAlerte -= HandleTargetSound;

       OnRagdoll -= EnableRagdollPhysics;
       OnRagdoll -= KoMode;
       OnWakeUp -= DisableRagdollPhysics;
    }

    private void EnableRagdollPhysics()
    {
       Agent.enabled = false;
       SetRagdollPhysics(true);
    }

    private void DisableRagdollPhysics()
    {
	    SetRagdollPhysics(false);
	    
	    if (NavMesh.SamplePosition(transform.position, out NavMeshHit hit, 5f, NavMesh.AllAreas))
	    {
		    transform.position = hit.position;
		    Agent.enabled = true;
		    Agent.Warp(hit.position);
	    }
	    else
	    {
		    Debug.LogWarning($"{name}: impossible de retrouver une position NavMesh valide après le ragdoll.");
	    }
    }

    private void SetRagdollPhysics(bool enableRagdoll)
    {
       if (ragdollRigidbodies != null)
       {
          foreach (Rigidbody rb in ragdollRigidbodies)
             rb.isKinematic = !enableRagdoll;
       }

       if (mainCollider != null)
          mainCollider.enabled = !enableRagdoll;
    }
    

    private void HandleTargetSeen(Vector3 pos, IDetected target)
    {
       if (PNJStates is SuspiciousState || PNJStates is ChaseState || PNJStates is SurpriseState)
          return;
       PnjGoTo(new SurpriseState(target));
    }

    private void HandleTargetSound(ISondDetected target, GameObject targetPosition)
    {
       if (PNJStates is SurpriseState || PNJStates is ChaseState || PNJStates is IntrigueState)
          return;
       PnjGoTo(new IntrigueState(target, targetPosition));
    }

    private void Start()
    {
       PnjGoTo(new PatrolState(this));
    }

    private void Update()
    {
       PNJStates.UpdateState(this);
    }

    public T GetAptitude<T>() where T : PnjAptitude
    {
       foreach (PnjAptitude aptitude in aptitudes)
       {
          if (aptitude is T match)
             return match;
       }
       return null;
    }

    public void PnjGoTo(IPnjStates state)
    {
       PNJStates?.ExitState(this);
       PNJStates = state;
       PNJStates?.EnterState(this);
       OnStatesChanged?.Invoke(PNJStates);
    }

    public void KoMode()
    {
       PnjGoTo(new KoState());
    }
}