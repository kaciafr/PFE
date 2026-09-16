using System;
using System.Collections.Generic;
using UnityEngine;

namespace Routine
{
	public class RoutineTime : MonoBehaviour
	{
		[SerializeField] private List<PointTime> pointTimes;
		public event Action<Vector3> OnPointTimeChange;

		public void Update()
		{
			for (int i = 0; i < pointTimes.Count; i++)
			{
				OnPointTimeChange?.Invoke(pointTimes[i].transform.position);
			}
		}
	}
}
