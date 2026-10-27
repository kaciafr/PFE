namespace Goblfin.PuzzleSystem
{
    public interface IAction
    {
        public void Enter();
        public void Exit(); 
        public void Tick();
        public bool isFinished { get; }
    }
}