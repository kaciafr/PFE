using CharacterController.Script;
using UnityEngine;

namespace PlayerSound
{
	public class NoiseRadius : MonoBehaviour
	{
		[SerializeField] private SphereCollider collider;
		[SerializeField] private CharacterSetup characterSetup;
		
		[Range(0f, 10f)]
		[field:SerializeField] public float NoiseEffect{get; private set;}

		private void Update()
		{
			collider.radius = characterSetup.MovementBase.controller.velocity.magnitude / NoiseEffect;
		}
	}
}