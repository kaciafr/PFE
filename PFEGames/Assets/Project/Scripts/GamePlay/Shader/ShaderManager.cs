 using System;
using Goblfin.CharactersSystem.Player.States;
using Goblfin.Player;
using Goblfin.Player.States;
using Goblfin.PnjSystem;
using UnityEngine;
namespace Goblfin.Gameplay
{
    public class ShaderManager : MonoBehaviour
    {
        public static ShaderManager Instance { get; private set; }

        [SerializeField] private PlayerManager player;
        public event Action NormalStateChanged;
        public event Action SearchStateChanged;
        public event Action DangerStateChanged;
        public event Action DieStateChanged;
        public event Action HideStateChanged;
        private bool otherThings = false;
        private IPnjStates state;

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
            NormalStateChanged?.Invoke();
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
                GobflinStateColor(brainPnj.PnjStates);
            }
        }

        private void GobflinStateColor(IPnjStates states)
        {
            state = states;
            switch (states)
            {
                case PatrolState:
                    if(!otherThings)
                        NormalStateChanged?.Invoke();
                    break;
                case SearchState:
                    if(!otherThings)
                        SearchStateChanged?.Invoke();
                    break;
                case ChaseState:
                    DangerStateChanged?.Invoke();
                    break;
                
            }
        }

        public void Hide(bool hide)
        {
            if(state is ChaseState) return;
            if (hide)
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