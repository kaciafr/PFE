using Goblfin;
using Runtime.Project.Scripts.Core.Utilities;
using UnityEngine;

namespace Runtime.Project.Scripts.Core
{
	public static class ElementDeath
	{
		public static GameObject Replace(ElementSimulation element, DeathVisual visual)
		{
			GameObject target = element.Root;
			if (!target.activeSelf) return null;  

			GameObject prefab = element.ElementData.GetDeathPrefab(visual);
			Transform t = target.transform;
			GameObject result = null;

			if (prefab != null)
			{
				result = PrefabPool.Get(prefab, t.position, t.rotation);
				result.transform.localScale = t.localScale;
			}

			PrefabPool.Release(target);
			return result;
		}
	}
}