using System;
using System.Collections.Generic;
using UnityEngine;

namespace Runtime.Project.Scripts.Core
{
	public class ElementSimulation : MonoBehaviour
	{
		[Header("References")]
		[field:SerializeField] public ElementData ElementData { get; private set; }
		[SerializeField] private List<ElementInfo> elementInfos =  new List<ElementInfo>();
		[field:SerializeField] public Collider Collider{ get; private set; }
		
		[Header("Dynamique Variables")] 
		[field:SerializeField] public float Heat{ get; set; }
		[field:SerializeField] public float Humidity{ get; set; }
		[field:SerializeField] public float Electricity{ get; set; }
		
		private float currentDegres;
		private IElementState currentState;
		private void Awake()
		{
			ElementInfo[] found = GetComponents<ElementInfo>();
			elementInfos.AddRange(found);
			foreach (var elementInfo in elementInfos)
			{
				elementInfo.Init(this);
			}
			Collider = GetComponent<Collider>();
		}

		public T GetInfo<T>() where T : ElementInfo
		{
			foreach (var elementInfo in elementInfos)
			{
				if(elementInfo is T info)
					return info;
			}

			return null;
		}

		public void Start()
		{
			ResetSimulation();
		}

		private void OnEnable() => ResetSimulation();

		private void ResetSimulation()
		{
			Heat = 0f;
			Humidity = 0f;
			Electricity = 0f;
			ChangeState(new NormalState());
		}

		private void Update()
		{
			LogicElement();
			currentState.Update(this);
			
			if(Heat <= 0.2f)
				return;
			
			LogicWeather();
		}
		
		public void ChangeState(IElementState state)
		{
			state.Exit(this);
			currentState = state;
			state.Enter(this);
		}
		
		private void LogicWeather()
		{
			float ambientHeat = 0f;       
			float ambientHumidity = 0f;      
    
			float coolingRate = 5f;
			float dryingRate = 2f;

			Heat = Mathf.MoveTowards(Heat, ambientHeat, coolingRate * Time.deltaTime);
			Humidity = Mathf.MoveTowards(Humidity, ambientHumidity, dryingRate * Time.deltaTime);
			
			float targetDegrees = ElementData.Normaldegres + Heat - (Humidity * 0.1f);
			currentDegres = Mathf.MoveTowards(currentDegres, targetDegrees, 10f * Time.deltaTime);
		}
		
		private void LogicElement()
		{
			float evaporation = (Heat * 0.1f + ElementData.VaporationSpeed) * Time.deltaTime;
			Humidity -= evaporation;
			
			float cooling = Humidity * 0.05f * Time.deltaTime;
			Heat -= cooling;
			
			if (Heat < 0.1f) 
				Heat = 0f;
			
			if (Electricity > 0f)
			{
				float conductivity = Mathf.Max(0.01f, ElementData.ElectricityConductibility);
				Electricity -= (1f / conductivity) * Time.deltaTime;

				if (Electricity < 0.1f) 
					Electricity = 0f;
			}
			Heat = Mathf.Clamp(Heat, 0f, 100f);
			Humidity = Mathf.Clamp(Humidity, 0f, 100f);
			Electricity = Mathf.Clamp(Electricity, 0f, 100f);
		}
		
		public void AddHeat(float heat) => Absorbe<HeatChannel>.Run(this, heat);
		public void AddHumidity(float humidity) => Absorbe<HumidityChannel>.Run(this, humidity);
		public void AddElectricity(float electricity) => Absorbe<ElectricityChannel>.Run(this, electricity);
		public void FirePropagation() => Propagation<HeatChannel>.Run(this);
		public void ElectricityPropagation() => Propagation<ElectricityChannel>.Run(this);
	}
}
