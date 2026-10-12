using Goblfin.PnjSystem.PnjDetection;
using UnityEngine;

namespace Runtime.Project.Scripts.Core
{
	public class ResolveProbleme : PnjAptitude
	{
		[SerializeField] private float range = 5f;
		[SerializeField] private float humidityPerSecond = 25f;
		
		public bool CloseToTheProbleme(ElementSimulation probleme)=> (probleme.transform.position -  transform.position).sqrMagnitude <= range;
		
		public void Resolve(ElementSimulation probleme) => probleme.AddHumidity(humidityPerSecond * 0.5f ); 
	}
}