using UnityEngine;

namespace Goblfin.SoundSystem
{
    public class PlaySoundState : StateMachineBehaviour
    {
        public SoundType soundType;

        public bool playSoundEnter = true;
        public bool playSoundExit = false;


        public override void OnStateEnter(
            Animator animator, 
            AnimatorStateInfo stateInfo, 
            int layerIndex)
        {
            if (playSoundEnter)
            {
                SoundManager.Instance.PlaySFX(soundType);
            }
        }

        public override void OnStateExit(
            Animator animator, 
            AnimatorStateInfo stateInfo, 
            int layerIndex)
        {
            if (playSoundExit)
            {
                SoundManager.Instance.PlaySFX(soundType);
            }
        }
    }
}