using UnityEngine;

namespace Utilities
{
	public class SuspiciousMeter
	{
		public float Value { get; private set; }

		private float increaseSpeed;
		private float decreaseSpeed;

		public SuspiciousMeter(float increaseSpeed, float decreaseSpeed)
		{
			this.increaseSpeed = increaseSpeed;
			this.decreaseSpeed = decreaseSpeed;
		}

		public void IncreaseFromDetection(float distanceFactor, float angleFactor, float deltaTime)
		{
			float speed = increaseSpeed * distanceFactor * angleFactor;
			Value = Mathf.Clamp01(Value + speed * deltaTime);
		}

		public void Decrease(float deltaTime)
		{
			Value = Mathf.Clamp01(Value - decreaseSpeed * deltaTime);
		}

		public void Reset()
		{
			Value = 0f;
		}
	}
}