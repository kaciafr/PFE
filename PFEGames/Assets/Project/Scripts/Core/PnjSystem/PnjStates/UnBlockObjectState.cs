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
            
            Communication.SomeoneIsIn(brainPnj);
            brainPnj.Agent.speed = AgentSpeed;
        }
        
        public void UpdateState(BrainPnj brainPnj)
        { 
            brainPnj.Animator.SetTrigger("Kicking");
            if(objectblock.DoorState is DoorBlockedState)
                objectblock.Interact();
            else
            {
                brainPnj.PnjGoTo(new PatrolState(brainPnj));
            }
        }

        public void ExitState(BrainPnj brainPnj)
        {
            Communication.EndAction(brainPnj);
        }
    }
}