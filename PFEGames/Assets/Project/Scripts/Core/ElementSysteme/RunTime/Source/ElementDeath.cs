using UnityEngine;

namespace Runtime.Project.Scripts.Core
{
	public static class ElementDeath
	{
		public static GameObject Replace(ElementSimulation element, DeathVisual visual)
		{
			ElementData data = element.ElementData;

			GameObject prefab = data.GetDeathPrefab(visual);

			Transform t = element.transform;
			GameObject result = null;

			if (prefab != null)
			{
				result = PrefabPool.Get(prefab,t.position,t.rotation);
				result.transform.localScale = t.localScale; 
			}

			PrefabPool.Release(element.gameObject);
			return result;
		}
	}
}