using System.Collections.Generic;
using Runtime.Project.Scripts.Core.ElementSysteme.RunTime;
using UnityEngine;

namespace Runtime.Project.Scripts.Core
{
	public class ElementSimulation : MonoBehaviour
	{
		[Header("References")]
		[field:SerializeField] public ElementData ElementData { get; private set; }
		private List<ElementInfo> elementInfos =  new List<ElementInfo>();
		[field:SerializeField] public Collider Collider{ get; private set; }
		
		[Header("Dynamique Variables")] 
		[field:SerializeField] public float TargetDegres{ get; private set; }
		[field:SerializeField] public float Heat{ get; private set; }
		[field:SerializeField] public float Humidity{ get; private set; }
		[field:SerializeField] public float Electricity{ get; private set; }
		
		private float currentDegres;
		private IElementState currentState;
		private void Awake()
		{
			GetComponents(elementInfos);
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

		private void OnEnable() => ResetSimulation();

		private void ResetSimulation()
		{
			Heat = 0f;
			Humidity = 0f;
			Electricity = 0f;
			TargetDegres = ElementData.Normaldegres;
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
			var weather = WhaterManager.Instance;
			float dt = Time.deltaTime;

			Heat = Mathf.MoveTowards(Heat, weather.standarDegres, 5f * dt);

			Heat -= weather.currrentRainIntensity * 10f * dt;

			float ambientHumidity = weather.currentHumidity * 10f;
			Humidity = Mathf.MoveTowards(Humidity, ambientHumidity, 10f * dt);

			TargetDegres = ElementData.Normaldegres + Heat - Humidity + weather.standarDegres * 0.2f;

			currentDegres = Mathf.MoveTowards(currentDegres, TargetDegres, 5f * dt);

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
