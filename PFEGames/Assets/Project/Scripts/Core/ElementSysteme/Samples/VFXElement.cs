using System;
using UnityEngine;

namespace Goblfin
{
    public class VFXElement : MonoBehaviour
    {
        private ElementSimulation elementSim;
        [SerializeField] private ParticleSystem phaseTwoParticle;
        [SerializeField] private ParticleSystem phaseOneParticle;

        private void Start()
        {
	        elementSim = GetComponent<ElementSimulation>();
		        phaseTwoParticle.Stop();
		        phaseOneParticle.Stop();
	        
        }
        private void Update()
        {
	        if (elementSim == null)
		        return;

	       if (elementSim.Heat >= elementSim.ElementData.Hot)
	        {
		        phaseOneParticle.Play();
		        phaseTwoParticle.Stop();
	        }
	        else if (elementSim.Heat < elementSim.ElementData.Hot)
	        {
		        phaseOneParticle.Stop();
	        }
	        
	        if(elementSim.CurrentState is BurningState)
		        phaseTwoParticle.Play();
	        else
		        phaseTwoParticle.Stop();
        }
    }
}
