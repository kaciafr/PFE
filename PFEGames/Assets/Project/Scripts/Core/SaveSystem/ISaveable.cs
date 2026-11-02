namespace Goblfin
{
    public interface ISaveable
    {
        string Id { get; }
        string CaptureState();
        void RestoreState(string state);
    }
}