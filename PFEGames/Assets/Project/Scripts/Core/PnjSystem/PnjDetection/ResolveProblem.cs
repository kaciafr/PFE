using Goblfin.PnjSystem.PnjDetection;
using UnityEngine;

namespace Goblfin.PnjSystem
{
	public class ResolveProblem : PnjAptitude
	{
		[SerializeField] private float range = 5f;
		[SerializeField] private float humidityPerSecond = 25f;
		
		public bool CloseToTheProbleme(ElementSimulation probleme)=> (probleme.transform.position -  transform.position).sqrMagnitude <= range;
		
		public void Resolve(ElementSimulation probleme) => probleme.AddHumidity(humidityPerSecond); 
	}
}