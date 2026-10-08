using System;

namespace DefaultNamespace.PuzzleSystem
{
    public interface ICondition
    {
        public  bool isMet { get; }
        event Action Changed;   

    }
}