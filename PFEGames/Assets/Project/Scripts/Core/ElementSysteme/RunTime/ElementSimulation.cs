using System.Collections.Generic;
using Runtime.Project.Scripts.Core;
using UnityEngine;

namespace Goblfin
{
	public class ElementSimulation : MonoBehaviour
	{
		[Header("References")]
		[field:SerializeField] public ElementData ElementData { get; private set; }
		[SerializeField] private List<ElementInfo> elementInfos =  new List<ElementInfo>();
		[field:SerializeField] public Collider Collider{ get; private set; }
		
		[Header("Dynamics Variables")] 
		[field:SerializeField] public float Heat{ get; set; }
		[field:SerializeField] public float Humidity{ get; set; }
		[field:SerializeField] public float Electricity{ get; set; }
		
		private float currentDegrees;
		public IElementState CurrentState { get; private set; }
		[Header("References")]
		[SerializeField] private GameObject root;
		public GameObject Root
		{
			get
			{
				if (root != null) return root;
				var pooled = GetComponentInParent<PooledInstance>();
				return pooled != null ? pooled.gameObject : gameObject;
			}
		}
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
			CurrentState.Update(this);
			
			if(Heat <= 0.2f)
				return;
			
			LogicWeather();
		}
		
		public void ChangeState(IElementState state)
		{
			state.Exit(this);
			CurrentState = state;
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
			
			float targetDegrees = ElementData.AverageDegree + Heat - (Humidity * 0.1f);
			currentDegrees = Mathf.MoveTowards(currentDegrees, targetDegrees, 10f * Time.deltaTime);
		}
		private void LogicElement()
		{
			var dt = Time.deltaTime;

			if (Humidity > 0f && Heat > 0f)
			{
				float evaporated = Mathf.Min(Humidity, Heat * ElementData.EvaporationSpeed * dt);
				Humidity -= evaporated;
				Heat -= evaporated * 2;  
			}

			if (Heat < 0.1f)
				Heat = 0f;

			if (Electricity > 0f)
			{
				float conductivity = Mathf.Max(0.01f, ElementData.ElectricalConduct);
				Electricity -= (1f / conductivity) * dt;

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
