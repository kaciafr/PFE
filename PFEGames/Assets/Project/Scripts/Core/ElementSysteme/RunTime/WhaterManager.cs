using System;
using UnityEngine;
using Random = System.Random;

namespace Runtime
{
	public class WhaterManager : MonoBehaviour
	{
		public static WhaterManager Instance;
		public WatherData watherData;

		[Header("WatherData")] 
		public float standarDegres = 23f;
		public float currentHumidity;
		public float currrentRainIntensity;
		public float currentWindSpeed;
		public float targetAmbient;
		[SerializeField] private float changeInterval = 20f;
		[SerializeField] private float speed = 0.08f;
		
		
		private float timer;
		private float targetTemperature;
		private float targetRain;
		private float targetWind;
		private float targetHumidity;
		
		public void Awake()
		{ 
			if (Instance != null && Instance != this)
				return;
			
			Instance = this;
		}

		private void Update()
		{
			timer += Time.deltaTime;

			if (timer > changeInterval)
			{
				ChangeWeather();
				timer = 0;
				return;
			}

			ApplyWather();
		}
		
		private void ChangeWeather()
		{
			targetHumidity = UnityEngine.Random.Range(0.05f, 1f);
			targetRain = targetHumidity > 0.7f ? UnityEngine.Random.Range(0f, 1f) : 0f;
			targetWind = UnityEngine.Random.Range(0f, 1f);

			targetTemperature = watherData.standarDegres;
			targetTemperature -= targetRain * 2f;
			targetTemperature -= targetWind * 1.4f;
			Debug.Log(targetTemperature);

			targetTemperature = Mathf.Clamp(targetTemperature, -50f, 50f);
		}

		private void ApplyWather()
		{
			currentHumidity = Mathf.Lerp(currentHumidity, targetHumidity, speed * Time.deltaTime);
			currrentRainIntensity = Mathf.Lerp(currrentRainIntensity, targetRain, speed * Time.deltaTime);
			currentWindSpeed = Mathf.Lerp(currentWindSpeed, targetWind, speed * Time.deltaTime);

			float targetAmbient = targetTemperature - currentHumidity * 2f - currentWindSpeed * 1.4f;
			standarDegres = Mathf.Lerp(standarDegres, targetAmbient, speed * Time.deltaTime);
			
		}
	}
}