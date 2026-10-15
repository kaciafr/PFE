using System.Collections.Generic;
using Runtime.Project.Scripts.Core;
using UnityEngine;
using UnityEngine.Pool;

namespace Goblfin
{
	public static class PrefabPool
	{
		private static readonly Dictionary<GameObject, ObjectPool<GameObject>> Pools = new();
		private static readonly HashSet<GameObject> Prewarmed = new();

		public static void Prewarm(GameObject prefab, int count)
		{
			ObjectPool<GameObject> pool = GetPool(prefab);
			var temp = new List<GameObject>(count);
			for (int i = 0; i < count; i++)
				temp.Add(pool.Get());         
			foreach (GameObject go in temp)
				pool.Release(go);
		}

		public static GameObject Get(GameObject prefab, Vector3 position, Quaternion rotation)
		{
			ObjectPool<GameObject> pool = GetPool(prefab);

			GameObject instance;
			do { instance = pool.Get(); }
			while (instance == null);          

			instance.transform.SetPositionAndRotation(position, rotation);
			instance.SetActive(true);         
			return instance;
		}

		private static ObjectPool<GameObject> GetPool(GameObject prefab)
		{
			if (Pools.TryGetValue(prefab, out var pool))
				return pool;

			pool = new ObjectPool<GameObject>(
				createFunc: () =>
				{
					GameObject go = Object.Instantiate(prefab);
					go.AddComponent<PooledInstance>().source = prefab;
					return go;
				},
				actionOnRelease: go => go.SetActive(false),
				actionOnDestroy: go => { if (go != null) Object.Destroy(go); },
				defaultCapacity: 8,
				maxSize: 64);

			Pools[prefab] = pool;
			return pool;
		}

		public static void Release(GameObject instance)
		{ 
			if (!instance.gameObject.activeSelf) return;
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
