namespace Goblfin
{
    public interface IDoorState
    {
        public void EnterState(DoorInteract door);
        public void UpdateState(DoorInteract door);
        public void ExitState(DoorInteract door);
        public void InteractTypeState(DoorInteract door);
    }
}