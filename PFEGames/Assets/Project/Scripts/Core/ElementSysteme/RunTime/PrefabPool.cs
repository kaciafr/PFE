using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace Runtime.Project.Scripts.Core
{
	public static class PrefabPool
	{
		private static readonly Dictionary<GameObject, ObjectPool<GameObject>> Pools = new();
		private static readonly HashSet<GameObject> Prewarmed = new();

		public static void Prewarm(GameObject prefab, int count)
		{
			var temp = new List<GameObject>(count);
			for (int i = 0; i < count; i++)
				temp.Add(Get(prefab, Vector3.zero, Quaternion.identity));
			foreach (GameObject go in temp)
				Release(go);
		}
	
		public static GameObject Get(GameObject prefab, Vector3 position, Quaternion rotation)
		{
			if (!Pools.TryGetValue(prefab, out ObjectPool<GameObject> pool))
			{
				pool = new ObjectPool<GameObject>(
					createFunc: () =>
					{
						GameObject go = Object.Instantiate(prefab);
						go.AddComponent<PooledInstance>().source = prefab;
						return go;
					},
					actionOnGet: go => go.SetActive(true),
					actionOnRelease: go => go.SetActive(false),
					actionOnDestroy: go => Object.Destroy(go),
					defaultCapacity: 8,
					maxSize: 64);

				Pools[prefab] = pool;
			}

			GameObject instance = pool.Get();
			instance.transform.SetPositionAndRotation(position, rotation);
			return instance;
		}

		public static void Release(GameObject instance)
		{
			if (instance.TryGetComponent(out PooledInstance pooled)
			    && Pools.TryGetValue(pooled.source, out ObjectPool<GameObject> pool))
			{
				pool.Release(instance);
			}
			else
			{
				Object.Destroy(instance); 
			}
		}

		
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
		private static void Reset() => Pools.Clear();
	}
	
}
