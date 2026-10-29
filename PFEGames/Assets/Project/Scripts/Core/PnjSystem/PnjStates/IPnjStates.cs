namespace Goblfin.PnjSystem
{
	public interface IPnjStates
	{
		StatePriority Priority { get; }
		float AgentSpeed { get; }
		public void EnterState(BrainPnj brainPnj);
		public void UpdateState(BrainPnj brainPnj);
		public void ExitState(BrainPnj brainPnj );
	}
}