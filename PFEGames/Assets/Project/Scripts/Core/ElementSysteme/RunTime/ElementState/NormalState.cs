using Runtime.Project.Scripts.Core;
using UnityEngine;
using UnityEngine.Audio;

namespace Goblfin
{
    public class NormalState : IElementState
    {
	    private Health currentHealth;
	    
        public void Enter(ElementSimulation element)
        {
        }

        public void Update(ElementSimulation element)
        {
            if (element.Heat > element.ElementData.Burn)
            {
                element.ChangeState(new BurningState());
            }
            
            if (element.Humidity > element.ElementData.Frozen)
            {
                if (!element.ElementData.IsLiquid)
                {
                    element.Collider.isTrigger = false;
                }
                element.ChangeState(new FrozenState());
            }
        }

        public void Exit(ElementSimulation element)
        {
        }
    }
}
