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
			LogicWather();
			LogicElement();
			currentState.Update(this);
		}
		
		public void ChangeState(IElementState state)
		{
			state.Exit(this);
			currentState = state;
			state.Enter(this);
		}
		
		private void LogicWather()
		{
			var weather = 0.5f;
			float dt = Time.deltaTime;

			Heat = Mathf.MoveTowards(Heat, weather, 5f * dt);

			Heat -= weather * 10f * dt;

			/*float ambientHumidity = weather * 10f;
			Humidity = Mathf.MoveTowards(Humidity, ambientHumidity, 10f * dt);*/

			var targetDegres = ElementData.Normaldegres + Heat - Humidity + weather * 0.2f;

			currentDegres = Mathf.MoveTowards(currentDegres, targetDegres, 5f * dt);

			Heat = Mathf.Clamp(Heat, 0, 100);
			Humidity = Mathf.Clamp(Humidity, 0, 100);
		}
		
		private void LogicElement()
		{
			float dt = Time.deltaTime;

			Heat -= Humidity * dt;

			Humidity -= Heat * ElementData.VaporationSpeed * dt;

			float electricityLoss = Electricity * (1f / ElementData.ElectricityConductibility) * dt;
			Electricity -= electricityLoss;

			if (Electricity < 0.1f)
				Electricity = 0f;


			Electricity = Mathf.Clamp(Electricity, 0, 100);
			Heat = Mathf.Clamp(Heat, 0, 100);
			Humidity = Mathf.Clamp(Humidity, 0, 100);

		}
		
		public void AddHeat(float heat) => Absorbe<HeatChannel>.Run(this, heat);
		public void AddHumidity(float humidity) => Absorbe<HumidityChannel>.Run(this, humidity);
		public void AddElectricity(float electricity) => Absorbe<ElectricityChannel>.Run(this, electricity);
		public void FirePropagation() => Propagation<HeatChannel>.Run(this);
		public void ElectricityPropagation() => Propagation<ElectricityChannel>.Run(this);
	}
}
