using System;
using DefaultNamespace.PuzzleSystem;
using UnityEngine;


    public class ConditionBase : MonoBehaviour , ICondition
    {
        public virtual bool isMet { get; }
        public event Action Changed; 
        protected void NotifyChanged() => Changed?.Invoke();   

    }
