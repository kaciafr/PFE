using UnityEngine;

namespace Goblfin.PnjSystem
{
	public interface IDetected
	{
		Transform transform { get; }
		float speed { get; }
	}
}