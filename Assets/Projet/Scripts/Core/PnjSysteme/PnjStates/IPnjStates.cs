namespace PnjStates
{
	public interface IPnjStates
	{
		public void EnterState(BrainPnj brainPnj);
		public void UpdateState(BrainPnj brainPnj);
		public void ExitState(BrainPnj brainPnj );
	}
}