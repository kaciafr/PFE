using UnityEngine;

namespace Goblfin
{
	public class ResolutionSource : MonoBehaviour
	{
		[field: SerializeField] public TypeOfResolution Type { get; private set; }

		private void OnEnable() => ResolveRegistry.Register(this);
		private void OnDisable() => ResolveRegistry.Unregister(this);
	}
}