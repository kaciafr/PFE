using System;
using UnityEngine;

namespace DefaultNamespace.PuzzleSystem
{
    public class CondtionBase : MonoBehaviour , ICondition
    {
        public bool isMet { get; }
        public event Action Changed; 
        protected void NotifyChanged() => Changed?.Invoke();   

    }
}