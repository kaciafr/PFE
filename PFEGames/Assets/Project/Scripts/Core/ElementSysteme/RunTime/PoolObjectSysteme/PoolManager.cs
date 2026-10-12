using System;
using System.Collections.Generic;
using UnityEngine;

namespace Runtime.Project.Scripts.Core.Utilities
{
    public class PoolManager : MonoBehaviour
    {
	    [Header("Scan")]
	    [SerializeField] private int maxPerPrefab = 32;

	    private void Start()
	    {
		    InitializePool();
	    }
	    
	    private void InitializePool()
	    {
		    var sims = FindObjectsByType<ElementSimulation>(FindObjectsInactive.Include, FindObjectsSortMode.None);
		    
		    var perData = new Dictionary<ElementData, int>();
		    foreach (ElementSimulation sim in sims)
		    {
			    if (sim.ElementData == null) continue;
			    perData[sim.ElementData] = perData.GetValueOrDefault(sim.ElementData) + 1;
		    }
		    
		    var needed = new Dictionary<GameObject, int>();
		    foreach (var (data, simCount) in perData)
		    {
			    int count = Mathf.Clamp(simCount, 1, data.PoolSize);
			    foreach (GameObject prefab in data.GetPoolPrefabs())
				    needed[prefab] = needed.GetValueOrDefault(prefab) + count;
		    }

		    foreach (var (prefab, count) in needed)
			    PrefabPool.Prewarm(prefab, Mathf.Min(count, maxPerPrefab));
	    }
    }
}
