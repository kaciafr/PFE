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
        [SerializeField ] private  List<CondtionBase> conditions;
        PuzzleState state =  PuzzleState.Locked;

        private void OnEnable() { foreach (var c in conditions) c.Changed += Evaluate; }


        public void Update()
        {
            if( state != PuzzleState.Solved ) return;
            foreach (var a in onSolved) a.Tick();
        }

        void OnDisable() { foreach (var c in conditions) c.Changed -= Evaluate; }
        void Evaluate()
        {
            if ( state == PuzzleState.Solved) return;
            if (conditions.All(c => c.isMet))
            {
                state = PuzzleState.Solved;
                foreach ( var  a  in onSolved ) a.Enter();
            }
        }
    }
}