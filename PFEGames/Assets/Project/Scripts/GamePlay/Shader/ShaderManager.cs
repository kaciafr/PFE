using System;
using Goblfin.CharactersSystem.Player.States;
using Goblfin.Player;
using Goblfin.PnjSystem;
using UnityEngine;

namespace Goblfin.Gameplay
{
    public class ShaderManager : MonoBehaviour
    {
        public static ShaderManager Instance { get; private set; }

        [SerializeField] private PlayerManager player;
        public event Action NormalStateChanged;
        public event Action DangerStateChanged;
        public event Action DieStateChanged;
        public event Action HideStateChanged;
        private bool isAnyPnjChasing = false;
        private bool otherThings = false;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

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
            isAnyPnjChasing = false;
            foreach (BrainPnj brainPnj in PnjRegistry.All)
            {
                if (brainPnj.PNJStates is ChaseState)
                {
                    isAnyPnjChasing = true;
                    break;
                }
            }
            
            if (isAnyPnjChasing)
            {
                DangerStateChanged?.Invoke();
            }
            else if(!isAnyPnjChasing && !otherThings)
            {
                NormalStateChanged?.Invoke();
            }
        }

        public void Hide(bool hide)
        {
            if (hide &&  !isAnyPnjChasing)
            {
                otherThings=true;
                HideStateChanged?.Invoke();
            }
            else
            {
                otherThings=false;
                NormalStateChanged?.Invoke();
            }
                
        }
    }
}