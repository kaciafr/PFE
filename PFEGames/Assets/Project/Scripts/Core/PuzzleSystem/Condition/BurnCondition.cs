using UnityEngine;

namespace Goblfin.PuzzleSystem
{
    public class BurnCondition :ConditionBase
    {
        [SerializeField] private ElementSimulation element; 
        private bool hasBurned;
        public override bool isMet => hasBurned;


        void Update()
        {
            if (hasBurned) return;

            if (element.CurrentState is BurningState burningState)
            {
                hasBurned = true;
                NotifyChanged();
            }
        }

    }
}