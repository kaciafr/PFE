using Goblfin.PnjSystem.WhatTheyDetect;
using UnityEngine;

namespace Goblfin.PnjSystem.PlayerSound
{
	public class NoiseRadius : MonoBehaviour
	{
		[SerializeField] private SphereCollider collider;
		[SerializeField] private GameObject noiseEffect;
		
		private IDetected target;
		[SerializeField] private float minSpeed;
		
		[Range(0f, 10f)]
		[field:SerializeField] public float NoiseEffect{get; private set;}

		private void Start()
		{
			target = GetComponentInParent<IDetected>();
		}
		private void Update()
		{
			// target.speed = niveau de BRUIT du joueur (voir PlayerManager.NoiseLevel) :
			// vitesse au sol, presque 0 accroupi, pic après une grosse réception
  			if (target.speed <= minSpeed || NoiseEffect <= 0f)   // NoiseEffect à 0 = division par zéro
			{
				noiseEffect.SetActive(false);
			}
			else
			{
				noiseEffect.SetActive(true);
				collider.radius = target.speed / NoiseEffect;
			}
			
		}
	}
}