using Goblfin.CharactersSystem.Player;
using UnityEngine;
using UnityEngine.Rendering;

namespace Goblfin.Player.Component
{
    public class SpeedEffect :  MonoBehaviour
    {
        [SerializeField] private float fadeSpeed = 3f;
        [SerializeField] private Volume volume;
        private PlayerManager playerManager;


        public void Start()
        {
            volume.weight = 0f;
            
        }

        private void Update()
        {
            float walk = playerManager.Settings.Movement.WalkSpeed;
            float sprint = playerManager.Settings.Movement.SprintSpeed;
            float wanted = Mathf.InverseLerp(walk, sprint , playerManager.HorizontalSpeed);
            
            volume.weight = Mathf.MoveTowards(volume.weight, wanted, fadeSpeed * Time.deltaTime);
        }
    }
}