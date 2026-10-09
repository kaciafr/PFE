using UnityEngine;

namespace Runtime.Project.Scripts.Core
{
    [CreateAssetMenu(fileName = "MaterialData")]
    public class ElementData : ScriptableObject
    {
	    [Header("Influence Feu")]
	    [field: SerializeField]
	    public float FireResistance { get; private set; } = 15;
        [field:SerializeField] public float Brule{ get; private set; } 
        
        [Header("Sond & Particule")]
        [field:SerializeField] public ParticleSystem phaseOneParticule;
        [field:SerializeField] public ParticleSystem phaseTwoParticule;
        
        [Header("Influence Eau")]
        [field:SerializeField]public float AbsorptionResistance{ get; private set; }
        [field:SerializeField] public float Frozen{ get; private set; }
        [field:SerializeField]public float VaporationSpeed{ get; private set; }
        
        [Header("Influence Électricité")]
        [field:SerializeField] public float Tazzer{ get; private set; }
        [field:SerializeField]public float ElectricityConductibility{ get; private set; }
        
        [Header("Sond & Particule")]
        [SerializeField] private ParticleSystem electroParticule;

        [Header("Structure")] 
        [field:SerializeField] public float Normaldegres{ get; private set; }
        [field:SerializeField]public float MaxHealth{ get; private set; }
        [field:SerializeField]public float DamageHealth{ get; private set; }
        [field:SerializeField]public bool IsLiquid{ get; private set; }
        
        [Header("Transformation")]
        [field:SerializeField]public GameObject Prefab{ get; private set; }
        [field:SerializeField]public GameObject BurnPrefab{ get; private set; }
        [field:SerializeField]public GameObject NormalPrefab{ get; private set; }
        [field:SerializeField]public GameObject FrozenPrefab{ get; private set; }
        
        public GameObject GetDeathPrefab(DeathVisual visual) => visual switch
        {
	        DeathVisual.Frozen => FrozenPrefab,
	        DeathVisual.Burned => BurnPrefab,
	        _                  => NormalPrefab
        };
    }
}
