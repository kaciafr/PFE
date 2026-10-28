using Goblfin.CharactersSystem.Player;
using Goblfin.SoundSystem;
using UnityEngine;

namespace Goblfin.Player.Component
{
    public class PlayerFootSteps : MonoBehaviour
    {
        [SerializeField, Range(0f, 1f)] private float minVolume = 0.2f;
        [SerializeField, Range(0f, 1f)] private float maxVolume = 1f;
        
        [SerializeField]private PlayerManager playerManager;

        private void OnEnable() => playerManager.AnimEvents.Footstep += PlayStep;
        private void OnDisable()=> playerManager.AnimEvents.Footstep -= PlayStep;

        private void PlayStep()
        {
            float t = playerManager.HorizontalSpeed / playerManager.Settings.Movement.SprintSpeed;
            float volume = Mathf.Lerp(minVolume, maxVolume, t);
            
            SoundManager.Instance.PlaySFX(SoundType.Footstep, volume);
        }
    }
}