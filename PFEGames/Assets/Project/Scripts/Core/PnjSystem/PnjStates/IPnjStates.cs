namespace Goblfin.PnjSystem
{
	public interface IPnjStates
	{
		StatePriority Priority { get; }
		public void EnterState(BrainPnj brainPnj);
		public void UpdateState(BrainPnj brainPnj);
		public void ExitState(BrainPnj brainPnj );
	}
}