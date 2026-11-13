namespace Goblfin.PnjSystem
{
    public class UnBlockObjectState : IPnjStates
    {
        public StatePriority Priority => StatePriority.UnBlockProblem;
        private DoorInteract objectblock;
        public float AgentSpeed => 0;

        public UnBlockObjectState(DoorInteract objectblock)
        {
            this.objectblock = objectblock;
        }
        
        public void EnterState(BrainPnj brainPnj)
        {
            brainPnj.Agent.speed = AgentSpeed;
        }

        public void UpdateState(BrainPnj brainPnj)
        {
            objectblock.Interact();
        }

        public void ExitState(BrainPnj brainPnj)
        {
            
        }
    }
}