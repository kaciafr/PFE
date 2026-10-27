using System;

namespace Goblfin.PuzzleSystem
{
    public interface ICondition
    {
        public  bool isMet { get; }
        event Action Changed;   

    }
}