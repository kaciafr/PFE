using CharacterController.Script;
using Script.PlayerControllers;

namespace InteractSysteme.Script
{
	public interface IInteractable
	{
		public int Priority { get; }
		void OnPlayerExit(CharacterSetup characterSetup);
		public void Interact(CharacterSetup characterSetup);
		public void OnPlayerEnter(CharacterSetup characterSetup);
	}
}