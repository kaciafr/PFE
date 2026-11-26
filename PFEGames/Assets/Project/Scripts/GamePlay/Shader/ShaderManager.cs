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
        private enum Alert { Normal, Search, Chase }
        private Alert lastAlert = (Alert)(-1);
        private bool lastDead, anyChase;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            
            NormalStateChanged?.Invoke();
        }

        private void Start()
        {
            NormalStateChanged?.Invoke();
        }
        
        private void Update()
        {
            bool dead = player != null && player.StateMachine.Current is PlayerDieState;

            Alert alert = Alert.Normal;
            foreach (BrainPnj pnj in PnjRegistry.All)
            {
                if (pnj.PnjStates is ChaseState) { alert = Alert.Chase; break; }
                if (pnj.PnjStates is SearchState) alert = Alert.Search;
            }
            anyChase = alert == Alert.Chase;

            if (dead) { if (!lastDead) DieStateChanged?.Invoke(); lastDead = true; lastAlert = (Alert)(-1); return; }
            if (lastDead) { lastDead = false; lastAlert = (Alert)(-1); }   

            if (alert == lastAlert) return;
            lastAlert = alert;
            Dispatch(alert);
        }

        /*private void GobflinStateColor(IPnjStates states)
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
        }*/

        private void Dispatch(Alert alert)
        {
            if (alert == Alert.Chase) { DangerStateChanged?.Invoke(); return; }
            if (otherThings) return;
            if (alert == Alert.Search) SearchStateChanged?.Invoke();
            else NormalStateChanged?.Invoke();
        }

        public void Hide(bool hide)
        {
            otherThings = hide && !anyChase;
            if (hide && !anyChase) HideStateChanged?.Invoke();
            else if (!hide) { lastAlert = (Alert)(-1); }
        }
    }
}