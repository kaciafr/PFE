namespace Runtime.Project.Scripts.Core
{
    public interface IElementState
    {
        void Enter(ElementSimulation element);
        void Update(ElementSimulation element);
        void Exit(ElementSimulation element);
    }
}
