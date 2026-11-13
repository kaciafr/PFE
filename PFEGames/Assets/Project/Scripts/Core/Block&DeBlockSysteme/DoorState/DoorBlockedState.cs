using System;
using PrimeTween;
using UnityEngine;

namespace Goblfin
{
    public class DoorBlockedState : IDoorState
    {
        private const float ShakeDuration = 0.25f;
        private const float ShakeFrequency = 25f;
        private const float MinShakeAngle = 1.5f;  
        private const float MaxShakeAngle = 4f;   

        private bool startToUnBlock;
        private float timeToUnBlock = 5f;
        private float currentTime;
        private Tween shake;

        public void EnterState(DoorInteract door)
        {
            startToUnBlock = false;
            currentTime = 0;
        }

        public void UpdateState(DoorInteract door)
        {
            if (startToUnBlock)
            {
                door.blockDoorSound.Play();
                currentTime += Time.deltaTime;
                if (currentTime >= timeToUnBlock)
                {
                    door.DoorGoTo(new DoorNormalState());
                    return;
                }
            }
            else
            {
                currentTime = Math.Max(0, currentTime - Time.deltaTime);
            }
            startToUnBlock = false;
        }

        public void ExitState(DoorInteract door)
        {
            if (shake.isAlive) shake.Stop();
        }

        public void InteractTypeState(DoorInteract door)
        {
            startToUnBlock = true;
            PlayShake(door);
        }

        private void PlayShake(DoorInteract door)
        {
            if (shake.isAlive) return;

            float progress = Mathf.Clamp01(currentTime / timeToUnBlock);
            float angle = Mathf.Lerp(MinShakeAngle, MaxShakeAngle, progress);

            shake = Tween.ShakeLocalRotation(
                door.transform,
                strength: new Vector3(0f, angle, 0f),
                duration: ShakeDuration,
                frequency: ShakeFrequency);
        }
    }
}