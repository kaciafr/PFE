using System.Collections.Generic;
using PnjDetection;
using UnityEngine;

public class BrainPnj : MonoBehaviour
{
	[SerializeField] private List<PnjAptitude> aptitudes = new List<PnjAptitude>();

	private void Start()
	{
		PnjAptitude[] found = GetComponents<PnjAptitude>();
		aptitudes.AddRange(found);
 
		foreach (PnjAptitude aptitude in aptitudes)
		{
			aptitude.Init(this);
		}
	}
}