using System.Collections;
using UnityEngine;

namespace Goblfin.PnjSystem
{
    public class PointTime : OverlapTrigger<BrainPnj>
    {
        [field: SerializeField] public float MinTime { get; private set; }
        [field: SerializeField] public Quaternion targetRotation;
        [SerializeField] private string targetAnimName;
        [SerializeField] private float animDuration = 1f;
        [SerializeField] private InteractPoint interactPoint;

        private Coroutine enterRoutine;

        protected override void OnZoneEnter(BrainPnj target)
        {
            base.OnZoneEnter(target);

            if (enterRoutine != null)
                StopCoroutine(enterRoutine);

            enterRoutine = StartCoroutine(PlayAnimThenInteract(target));
        }

        private IEnumerator PlayAnimThenInteract(BrainPnj target)
        {
            target.Animator.SetTrigger(targetAnimName);

            yield return null;

            AnimatorStateInfo stateInfo = target.Animator.GetCurrentAnimatorStateInfo(0);
            float waitTime = stateInfo.length > 0f && !stateInfo.loop ? stateInfo.length : animDuration;

            yield return new WaitForSeconds(waitTime);

            if (interactPoint != null)
                interactPoint.Interact();

            enterRoutine = null;
        }

        protected override void OnZoneExit(BrainPnj target)
        {
            base.OnZoneExit(target);

            if (enterRoutine != null)
            {
                StopCoroutine(enterRoutine);
                enterRoutine = null;
            }

            target.Animator.SetTrigger(targetAnimName);
        }
    }
}