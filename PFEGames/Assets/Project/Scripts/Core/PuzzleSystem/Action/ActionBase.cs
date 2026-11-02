using UnityEngine;

namespace Goblfin.PuzzleSystem
{
    public abstract class ActionBase : MonoBehaviour, IAction
    {
        public bool isFinished { get; protected set; }

        public virtual void Enter() => isFinished = false;
        public virtual void Tick() { }
        public virtual void Exit() { }
        
        public virtual void Undo(){}
    }
}