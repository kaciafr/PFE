namespace Goblfin
{
    public class DoorNormalState : IDoorState
    {
        public void EnterState(DoorInteract door)
        {
            
        }

        public void UpdateState(DoorInteract door)
        {
        }

        public void ExitState(DoorInteract door)
        {
            
        }

        public void InteractTypeState(DoorInteract door)
        {
            door.isOpen = true;
        }
    }
}