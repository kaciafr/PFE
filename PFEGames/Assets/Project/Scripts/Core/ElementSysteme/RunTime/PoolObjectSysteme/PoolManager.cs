using System.Collections.Generic;
using Goblfin.PnjSystem;
using UnityEngine;

namespace Goblfin
{
    public class PoolManager : MonoBehaviour
    {
        [Header("Scan")]
        [SerializeField] private int maxPerPrefab = 64;

        private void Start()
        {
            var needed = new Dictionary<GameObject, int>();

            CountElementPrefabs(needed);
            CountRockPrefabs(needed);

            foreach (var (prefab, count) in needed)
                PrefabPool.Prewarm(prefab, Mathf.Min(count, maxPerPrefab));
        }

        private static void CountElementPrefabs(Dictionary<GameObject, int> needed)
        {
            var sims = FindObjectsByType<ElementSimulation>(FindObjectsInactive.Include, FindObjectsSortMode.None);

            var perData = new Dictionary<ElementData, int>();
            foreach (ElementSimulation sim in sims)
            {
                if (sim.ElementData == null) continue;
                perData[sim.ElementData] = perData.GetValueOrDefault(sim.ElementData) + 1;
            }

            foreach (var (data, simCount) in perData)
            {
                int count = Mathf.Clamp(simCount, 1, data.PoolSize);
                foreach (GameObject prefab in data.GetPoolPrefabs())
                    needed[prefab] = needed.GetValueOrDefault(prefab) + count;
            }
        }

        private static void CountRockPrefabs(Dictionary<GameObject, int> needed)
        {
            var throwers = FindObjectsByType<ThrowThingsManager>(FindObjectsInactive.Include, FindObjectsSortMode.None);

            foreach (ThrowThingsManager thrower in throwers)
            {
                if (thrower.RockPrefab == null) continue;
                needed[thrower.RockPrefab] = needed.GetValueOrDefault(thrower.RockPrefab) + thrower.MaxRocks;
            }
        }
    }
}