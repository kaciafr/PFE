using System.Collections.Generic;
using Unity.Scripting.LifecycleManagement;
using UnityEngine;

namespace Goblfin.PnjSystem
{
    [AutoStaticsCleanup]
    public static partial class OverlapCache<T> where T : class
    {
        private static Dictionary<GameObject, T> cache = new Dictionary<GameObject, T>();

        // ReSharper disable Unity.PerformanceAnalysis
        private static bool Register(GameObject gameObject, out T result)
        {
            result = gameObject.GetComponentInChildren<T>(false);

            var marker = gameObject.AddComponent<OverlapCacheMarker>();
            marker.Setup(Unregister);
            
            Debug.Log($"Registering  {marker.gameObject.name} for {typeof(T).Name} overlaps detection");
            return cache.TryAdd(gameObject, result);
        }

        private static bool Unregister(GameObject gameObject)
        {
            Debug.Log($"Unregistering  {gameObject.name} for {typeof(T).Name} overlaps detection");
            return Unregister(gameObject, out _);
        }

        private static bool Unregister(GameObject gameObject, out T result)
        {
            return cache.Remove(gameObject, out result);
        }

        public static bool TryGet(GameObject gameObject, out T result)
        {
            if (!cache.TryGetValue(gameObject, out result))
                Register(gameObject, out result);

            return result != null;
        }
    }
}