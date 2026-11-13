
using UnityEngine;

namespace Goblfin
{
    public class DoorBlockedState : IDoorState
    {
        private bool startToUnBlock;
        private float timeToDeBlock = 5f;
        private float currentTime;

        public void EnterState(DoorInteract door)
        {
            startToUnBlock = false;
            currentTime = 0;
        }

        public void UpdateState(DoorInteract door)
        {
            if (startToUnBlock)
            {
                currentTime += Time.deltaTime;
                if (currentTime >= timeToDeBlock)
                {
                    door.DoorGoTo(new DoorNormalState());
                }
            }
        }

        public void ExitState(DoorInteract door)
        {
        }

        public void InteractTypeState(DoorInteract door)
        {
            startToUnBlock = true;
        }
    }
}