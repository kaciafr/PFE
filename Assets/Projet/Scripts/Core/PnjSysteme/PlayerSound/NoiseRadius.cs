using CharacterController.Script;
using UnityEngine;

namespace PlayerSound
{
	public class NoiseRadius : MonoBehaviour
	{
		[SerializeField] private SphereCollider collider;
		[SerializeField] private GameObject noiseEffect;
		
		[SerializeField] private CharacterSetup characterSetup;
		[SerializeField] private float minSpeed;
		
		[Range(0f, 10f)]
		[field:SerializeField] public float NoiseEffect{get; private set;}

		private void Update()
		{
			if (characterSetup.MovementBase.controller.velocity.magnitude <= minSpeed)
			{
				noiseEffect.SetActive(false);
			}
			else
			{
				noiseEffect.SetActive(true);
				collider.radius = characterSetup.MovementBase.controller.velocity.magnitude / NoiseEffect;
			}
			
		}
	}
}