using UnityEngine;

namespace Runtime.Project.Scripts.Core
{
    public class NormalState : IElementState
 
    {
	    private Health currentHealth;
        public void Enter(ElementSimulation element)
        {
            Debug.Log("Entering NormalState");
        }

        public void Update(ElementSimulation element)
        {
            if (element.Heat > element.ElementData.Brule || element.TargetDegres >= element.ElementData.Brule)
            {
                element.ChangeState(new BurningState());
            }
            

            if (element.Humidity > element.ElementData.Frozen && element.TargetDegres <= 5f)
            {
                if (!element.ElementData.IsLiquid)
                {
                    element.Collider.isTrigger = false;
                }
                element.ChangeState(new FrozenState());
            }
            else
            {
                element.Collider.isTrigger = true;
            }
        }

        public void Exit(ElementSimulation element)
        {
        }
    }
}
