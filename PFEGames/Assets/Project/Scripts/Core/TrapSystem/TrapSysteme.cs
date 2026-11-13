using System;
using UnityEngine;

namespace Goblfin.TrapSystem
{
    public class TrapSysteme : OverlapTrigger<IPrey>
    {
        public event Action TrapTriggered;
        public event Action TrapOpen;

        [SerializeField] private float cooldown = 3f;

        private bool isClosed;
        private float currentTime;

        protected override bool ShouldCheck => !isClosed;

        protected override void Update()
        {
            if (isClosed)
            {
                currentTime += Time.deltaTime;
                if (currentTime >= cooldown)
                {
                    isClosed = false;
                    currentTime = 0;
                    TrapOpen?.Invoke();
                }
                return;
            }

            base.Update();
        }

        protected override void OnZoneEnter(IPrey target) => Trigger(target);

        protected override void OnZoneStay(IPrey target) => Trigger(target);

        private void Trigger(IPrey target)
        {
            if (isClosed) return;

            target.Catches();
            isClosed = true;
            TrapTriggered?.Invoke();
        }
        
#if UNITY_EDITOR
        protected override Color GizmoColor =>
            Application.isPlaying && isClosed ? Color.red : Color.green;
#endif
    }
}