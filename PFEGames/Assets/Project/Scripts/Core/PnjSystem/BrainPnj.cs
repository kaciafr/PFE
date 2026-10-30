using System;
using System.Collections.Generic;
using Goblfin.PnjSystem.PnjDetection;
using Goblfin.PnjSystem.Routine;
using UnityEngine;
using UnityEngine.AI;

namespace Goblfin.PnjSystem
{
	public class BrainPnj : RagDoll
	{
		//A ENLEVER
		public Material material;
		[SerializeField] private List<PnjAptitude> aptitudes = new List<PnjAptitude>();
		[field:SerializeField] public NavMeshAgent Agent {get; private set;}
		[field:SerializeField] public List<PointTime> RoutinePattern {get; private set;} = new List<PointTime>();
		public int currentStep;
		[field:SerializeField] public Animator Animator {get; private set;}
		public event Action<IPnjStates> OnStatesChanged;
	
		private Rigidbody[] ragdollRigidbodies;
		private Collider mainCollider;
		private ProblemAttraction findProblem;
		
		public IPnjStates PnjStates {get; private set;}
		private IPnjStates pendingRequest;

		private AuditionCast audition;
		private VisionCone vision;

		#region Configuration

		private void Awake()
		{
			Animator = GetComponentInChildren<Animator>();
			Agent = GetComponent<NavMeshAgent>();

			ragdollRigidbodies = GetComponentsInChildren<Rigidbody>();
			mainCollider = GetComponent<Collider>();

			SetRagdollPhysics(false);

			PnjAptitude[] found = GetComponents<PnjAptitude>();
			aptitudes.AddRange(found);

			foreach (PnjAptitude aptitude in aptitudes)
			{
				aptitude.Init(this);
			}
		}
		
		private void Start()
		{
			//A ENLEVER
			material.color = Color.red;
			
			PnjGoTo(new PatrolState(this));
			findProblem = GetAptitude<ProblemAttraction>();
			vision = GetAptitude<VisionCone>();
			if (vision == null)
			{
				Debug.Log("No vision found");
			}
			audition = GetAptitude<AuditionCast>();
			if (audition == null)
			{
				Debug.Log("No audition found");
			}
			vision.OnTargetSeen += HandleTargetSeen;
			audition.OnHearAlerte += HandleTargetSound;
			if (findProblem == null)
			{
				Debug.Log("IL MANQUE UN TRUC LA");
			}
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
		
		private void OnEnable()
		{
			PnjRegistry.Register(this);
			OnRagdoll += KoMode;
			OnWakeUp += DisableRagdollPhysics;
		}
		private void OnDisable()
		{
			vision.OnTargetSeen -= HandleTargetSeen;
			audition.OnHearAlerte -= HandleTargetSound;
			
			OnRagdoll -= KoMode;
			OnWakeUp -= DisableRagdollPhysics;
			PnjRegistry.Unregister(this);
		}

		#endregion
		#region Ragdoll
		private void DisableRagdollPhysics()
		{
			Agent.enabled = true;
			Transform hips = Animator.GetBoneTransform(HumanBodyBones.Hips);
			Vector3 hipsPos = hips.position;
			Quaternion hipsRot = hips.rotation;

			SetRagdollPhysics(false);

			if (NavMesh.SamplePosition(hipsPos, out NavMeshHit hit, 5f, NavMesh.AllAreas))
			{
				Agent.enabled = false;
				
				Quaternion yaw = Quaternion.Euler(0f, hipsRot.eulerAngles.y, 0f);
				transform.SetPositionAndRotation(hit.position, yaw);
		    
				hips.SetPositionAndRotation(hipsPos, hipsRot);

				Agent.Warp(hit.position);
			}
		}

		public void SetRagdollPhysics(bool enableRagdoll)
		{
			if (ragdollRigidbodies != null)
			{
				foreach (Rigidbody rb in ragdollRigidbodies)
					rb.isKinematic = !enableRagdoll;
			}

			if (mainCollider != null)
				mainCollider.enabled = !enableRagdoll;
		}
		#endregion
		private void Update()
		{
			if (Agent.remainingDistance<=0.1)
			{
				Agent.speed = 0;
			}
			else
			{
				Agent.speed = PnjStates.AgentSpeed;
			}
			PnjStates.UpdateState(this);
		}
		private void FixedUpdate()
		{
			if (findProblem == null || !CanEnter(StatePriority.Resolve)) return;

			if (findProblem.TryFindProblem(out ElementSimulation element, out TypeOfProblem type) && !Communication.SomeoneIsIn(this))
				RequestState(new FindAResolveState(element, type));
			if (CanEnter(StatePriority.Following) && TryFindChasingFriend(out BrainPnj friend))
				RequestState(new FollowFriend(friend));
			
		}
		#region StateMachinManagement

		public void PnjGoTo(IPnjStates state)
		{
			PnjStates?.ExitState(this);
			PnjStates = state;
			PnjStates?.EnterState(this);
			OnStatesChanged?.Invoke(PnjStates);
		}
		
		private bool CanEnter(StatePriority priority) => priority > PnjStates.Priority;

		public void RequestState(IPnjStates requested)
		{
			if (pendingRequest == null || requested.Priority > pendingRequest.Priority)
				pendingRequest = requested;
			ApplyPendingRequest();
		}
  
		public void ForceState(IPnjStates state)
		{
			pendingRequest = null;
			PnjGoTo(state);
		}

		private void ApplyPendingRequest()
		{
			if (pendingRequest == null) return;

			IPnjStates requested = pendingRequest;
			pendingRequest = null;

			if (CanEnter(requested.Priority))
				PnjGoTo(requested);
		}
		private void KoMode() => ForceState(new KoState());
		#endregion
		#region PnjAptitude
		
		private void HandleTargetSeen(Vector3 pos, IDetected target)
		{
			if (!CanEnter(StatePriority.Suspicious)) return;
			RequestState(new SurpriseState(target));
		}

		private void HandleTargetSound(ISondDetected target, Transform source)
		{
			if (!CanEnter(StatePriority.Intrigue)) return;
			RequestState(new IntrigueState(target, source));
		}
		
		private bool TryFindChasingFriend(out BrainPnj friend)
		{
			friend = null;

			VisionCone look = GetAptitude<VisionCone>();
			if (look == null) return false;

			IReadOnlyList<BrainPnj> all = PnjRegistry.All;
			for (int i = 0; i < all.Count; i++)
			{
				BrainPnj other = all[i];
				if (other == this) continue;

				if (look.SeeFriendChasing(other))
				{
					friend = other;
					return true;
				}
			}
			return false;
		}
		#endregion
	}
}