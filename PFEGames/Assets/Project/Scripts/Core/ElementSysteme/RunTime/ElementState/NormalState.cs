using UnityEngine;

namespace Runtime.Project.Scripts.Core
{
    public class NormalState : IElementState
 
    {
	    private Health currentHealth;
        public void Enter(ElementSimulation element)
        {
        }

        public void Update(ElementSimulation element)
        {
            if (element.Heat > element.ElementData.Brule)
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
