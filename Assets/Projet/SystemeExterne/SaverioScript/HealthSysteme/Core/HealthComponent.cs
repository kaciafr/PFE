using System;
using UnityEngine;

public class HealthComponent : MonoBehaviour
{
	[SerializeField] private HealthData healthData;
	[field: SerializeField] public float CurrentHealth { get; private set; }
	private float MaxHealth => healthData.maxHealth;

	private void Start()
	{
		CurrentHealth = healthData.maxHealth;
	}
	
	public void AddOrRemoveHealth(float amount)
	{
		CurrentHealth += amount;
		
		if (CurrentHealth > healthData.maxHealth)
			CurrentHealth = healthData.maxHealth;
		
		CurrentHealth = Mathf.Clamp(CurrentHealth, 0, healthData.maxHealth);
	}
}