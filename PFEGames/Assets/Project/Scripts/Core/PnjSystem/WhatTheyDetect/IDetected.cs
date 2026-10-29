using UnityEngine;

namespace Goblfin.PnjSystem
{
	public interface IDetected
	{
		float VisibilityState { get; }
		Transform transform { get; }
		float speed { get; }
	}
}