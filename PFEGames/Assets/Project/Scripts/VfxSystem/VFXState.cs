using UnityEngine;

namespace Goblfin.VfxSystem
{
    public class VFXState : StateMachineBehaviour
    {
        public VFXType vfxType;
        public bool StopOnExit = false;
        private ParticleSystem vfx;

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            vfx = VfxManager.Instance.PlayVFX(vfxType, animator.transform.position);
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (StopOnExit && vfx != null)
            {
                vfx.Stop();
                vfx = null;
            }
        }
    }
}
