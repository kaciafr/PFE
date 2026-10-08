using UnityEngine;

namespace DefaultNamespace.PuzzleSystem
{
    
    public class ActionBase : MonoBehaviour , IAction
    {
        public void Enter()
        {
            throw new System.NotImplementedException();
        }

        public void Exit()
        {
            throw new System.NotImplementedException();
        }

        public void Tick()
        {
            throw new System.NotImplementedException();
        }

        public bool isFinished { get; }
    }
}