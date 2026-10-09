using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DefaultNamespace.PuzzleSystem
{
    
    public enum PuzzleState {Solved ,Locked }
    public class PuzzleController :  MonoBehaviour
    {
        [SerializeField ] private  List<ActionBase> onSolved;
        [SerializeField ] private  List<ConditionBase> conditions;
        PuzzleState state =  PuzzleState.Locked;
        [SerializeField] private bool reversible;

        private void OnEnable() { foreach (var c in conditions) c.Changed += Evaluate; }


        public void Update()
        {
            if( state != PuzzleState.Solved ) return;
            foreach (var a in onSolved) a.Tick();
        }

        void OnDisable() { foreach (var c in conditions) c.Changed -= Evaluate; }
        
        void Evaluate()
        {
            bool met = conditions.All(c => c.isMet);

            if (met && state == PuzzleState.Locked)
            {
                state = PuzzleState.Solved;
                foreach (var a in onSolved) a.Enter();
            }
            else if (!met && state == PuzzleState.Solved && reversible)
            {
                state = PuzzleState.Locked;
                foreach (var a in onSolved) a.Undo();
            }
        }
    }
}