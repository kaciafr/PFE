using UnityEngine;

namespace Goblfin.PnjSystem.Routine
{
	public class PointTime : MonoBehaviour
	{
		[field: SerializeField] public float MinTime{get; private set;}
		[field: SerializeField] public Quaternion targetRotation;
	}
}
