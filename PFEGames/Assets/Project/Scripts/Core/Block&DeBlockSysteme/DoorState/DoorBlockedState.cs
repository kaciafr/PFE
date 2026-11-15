using System;
using PrimeTween;
using UnityEngine;

namespace Goblfin
{
    public class DoorBlockedState : IDoorState
    {
        private const float ShakeDuration = 0.3f;
        private const float ShakeFrequency = 20f;
        private const float PauseBetweenAttempts = 0.15f;
        private const float MinShakeAngle = 6f;   
        private const float MaxShakeAngle = 16f;   
        private const float PositionShake = 0.01f;

        private bool startToUnBlock;
        private float timeToUnBlock = 5f;
        private float currentTime;
        private float nextShakeTime;
        private Tween rotationShake;
        private Tween positionShake;

        public void EnterState(DoorInteract door)
        {
            startToUnBlock = false;
            currentTime = 0;
            nextShakeTime = 0;
        }

        public void UpdateState(DoorInteract door)
        {
            if (startToUnBlock)
            {
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
            if (rotationShake.isAlive) rotationShake.Complete();
            if (positionShake.isAlive) positionShake.Complete();
        }

        public void InteractTypeState(DoorInteract door)
        {
            startToUnBlock = true;
            PlayAttempt(door);
        }

        private void PlayAttempt(DoorInteract door)
        {
            if (Time.time < nextShakeTime) return;
            nextShakeTime = Time.time + ShakeDuration + PauseBetweenAttempts;

            float progress = Mathf.Clamp01(currentTime / timeToUnBlock);
            float angle = Mathf.Lerp(MinShakeAngle, MaxShakeAngle, progress);
            float push = PositionShake * (1f + progress);

            if (door.blockDoorSound != null)
                door.blockDoorSound.Play();

            rotationShake = Tween.PunchLocalRotation(
                door.transform,
                strength: new Vector3(0f, angle, 0f),
                duration: ShakeDuration,
                frequency: ShakeFrequency);

            positionShake = Tween.ShakeLocalPosition(
                door.transform,
                strength: new Vector3(push, 0f, push),
                duration: ShakeDuration,
                frequency: ShakeFrequency);
        }
    }
}