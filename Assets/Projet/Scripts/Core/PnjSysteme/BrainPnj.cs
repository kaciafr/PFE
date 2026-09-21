using System;
using System.Collections.Generic;
using CharacterController.Script;
using PnjDetection;
using PnjStates;
using Routine;
using UnityEngine;
using UnityEngine.AI;

public class BrainPnj : MonoBehaviour
{
	[SerializeField] private List<PnjAptitude> aptitudes = new List<PnjAptitude>();
	
	[field:SerializeField] public NavMeshAgent Agent {get; private set;}
	[field:SerializeField] public List<PointTime> firstRoutine = new List<PointTime>();
	[field:SerializeField] public int currentStep = 0;
	
	public IPnjStates PNJStates { get; private set; }
	public event Action<IPnjStates> OnStatesChanged;
	
	private void Awake()
	{
		Agent = GetComponent<NavMeshAgent>();
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
	}


	private void HandleTargetSeen(Vector3 pos , CharacterSetup player)
	{
		if (PNJStates is SurpriseState || PNJStates is ChaseState)
			return;
		PnjGoTo(new SurpriseState(player));
	}
	private void HandleTargetSound(CharacterSetup player)
	{
		if (PNJStates is SurpriseState || PNJStates is ChaseState || PNJStates is IntrigueState)
			return;
		PnjGoTo(new IntrigueState(player));
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
			if(aptitude is T match)
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
	private void OnDisable()
	{
		VisionCone vision = GetAptitude<VisionCone>();
		if (vision != null)
			vision.OnTargetSeen -= HandleTargetSeen;
	    
		AuditionCast audition = GetAptitude<AuditionCast>();
		if (audition != null)
			audition.OnHearAlerte -= HandleTargetSound;
	}
}