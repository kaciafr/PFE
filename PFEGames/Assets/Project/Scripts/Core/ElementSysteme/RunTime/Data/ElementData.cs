using System.Collections.Generic;
using Runtime.Project.Scripts.Core;
using UnityEngine;

namespace Goblfin
{
    [CreateAssetMenu(fileName = "MaterialData")]
    public class ElementData : ScriptableObject
    {
	   
	    [Header("Influence Feu")]
	    [field: SerializeField]
	    public float FireResistance { get; private set; } = 15;

	    [field: SerializeField] public float Hot { get; private set; } = 15; 
        [field:SerializeField] public float Burn{ get; private set; } 
        
        [Header("Sound & Particle")]
        [field:SerializeField] public ParticleSystem phaseOneParticle;
        [field:SerializeField] public ParticleSystem phaseTwoParticle;

        [field:SerializeField] public float Frozen{ get; private set; }
        [field:SerializeField]public float EvaporationSpeed{ get; private set; }
        
        [Header("Influence Electricity")]
        [field:SerializeField] public float Electrify{ get; private set; }
        [field:SerializeField]public float ElectricalConduct{ get; private set; }
        
        [Header("Sound & Particle")]
        [SerializeField] private ParticleSystem electroParticle;

        [Header("Structure")] 
        [field:SerializeField] public float AverageDegree{ get; private set; }
        [field:SerializeField]public float MaxHealth{ get; private set; }
        [field:SerializeField]public float HitPower{ get; private set; }
        [field:SerializeField]public bool IsLiquid{ get; private set; }

        [field:SerializeField]public GameObject BurnPrefab{ get; private set; }
        [field:SerializeField]public GameObject NormalPrefab{ get; private set; }
        [field:SerializeField]public GameObject FrozenPrefab{ get; private set; }
        
        [Header("Pool")]
        [field: SerializeField] public int PoolSize { get; private set; } = 8;
        public IEnumerable<GameObject> GetPoolPrefabs()
        {
	        if (BurnPrefab != null)   yield return BurnPrefab;
	        if (FrozenPrefab != null) yield return FrozenPrefab;
	        if (NormalPrefab != null) yield return NormalPrefab;
        }
        public GameObject GetDeathPrefab(DeathVisual visual) => visual switch
        {
	        DeathVisual.Frozen => FrozenPrefab,
	        DeathVisual.Burned => BurnPrefab,
	        _                  => NormalPrefab
        };
    }
}
