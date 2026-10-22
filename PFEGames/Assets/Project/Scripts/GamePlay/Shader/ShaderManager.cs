using System;
using Goblfin.CharactersSystem.Player;
using Goblfin.CharactersSystem.Player.States;
using Goblfin.Player;
using Goblfin.PnjSystem;
using UnityEngine;

namespace Goblfin.Gameplay.Gameplay.Shader
{
    public class ShaderManager : MonoBehaviour
    {
        [SerializeField] private PlayerManager player;
        
        public event Action NormalStateChanged;
        public event Action DangerStateChanged;
        public event Action DieStateChanged;
        private bool isAnyPnjChasing = false;

        private void Start()
        {
            isAnyPnjChasing = false;
        }
        private void Update()
        {
            if (player != null && player.StateMachine.Current is PlayerDieState)
            {
                DieStateChanged?.Invoke();
                return;
            }
            foreach (BrainPnj brainPnj in PnjRegistry.All)
            {
                if (brainPnj.PNJStates is ChaseState)
                {
                    isAnyPnjChasing = true;
                    break; 
                }
                else
                {
                    isAnyPnjChasing = false;
                }
            }
            
            if (isAnyPnjChasing)
            {
                DangerStateChanged?.Invoke();
            }
            else
            {
                NormalStateChanged?.Invoke();
            }
        }
    }
}