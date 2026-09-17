using System;
using System.Collections.Generic;
using PnjDetection;
using PnjStates;
using Routine;
using UnityEngine;

public class BrainPnj : MonoBehaviour
{
	[SerializeField] private List<PnjAptitude> aptitudes = new List<PnjAptitude>();
	
	public IPnjStates PNJStates { get; private set; }
	public event Action<IPnjStates> OnStatesChanged;
	
	

	private void Start()
	{
		PnjAptitude[] found = GetComponents<PnjAptitude>();
		aptitudes.AddRange(found);
 
		foreach (PnjAptitude aptitude in aptitudes)
		{
			aptitude.Init(this);
		}
		
		PnjGoTo(new PatrolState(GetAptitude<PnjMove>()));
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
}