namespace Goblfin.CharactersSystem.Player.Interface
{
    public interface IInteractable
    {
        bool CanInteract  { get; }
        void Interact();
    }
}