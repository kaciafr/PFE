using Goblfin.CharactersSystem.Player.Interface;
using Goblfin.Player;
using PrimeTween;
using UnityEngine;

namespace Goblfin.Gameplay.CachetteZone
{
    public class OpenCupBoard : MonoBehaviour, IInteractable, IHideSpot
    {
        public bool occupied;
        private bool busy;
        public bool CanInteract => !occupied && !busy && IsPlayerInFront();

        [SerializeField] private int priority = 0;
        public int Priority => priority;

        [Header("Références")]
        [SerializeField] private Transform doorLeft;
        [SerializeField] private Transform doorRight;
        [SerializeField] private Transform insidePoint;
        [SerializeField] private Transform exitPoint;

        [Header("Réglages PrimeTween")]
        [SerializeField] private float openAngle = 95f;
        [SerializeField] private float openDuration = 0.4f;
        [SerializeField] private float closeDuration = 0.3f;
        [SerializeField] private float movePlayerDuration = 0.35f;
        [SerializeField] private Ease doorEase = Ease.OutBack;
        [SerializeField] private float exitTurnAngle = 180f;

        private PlayerManager playerManager;
        private Quaternion closeRotationLeft;
        private Quaternion closeRotationRight;
        private Quaternion openRotationLeft;
        private Quaternion openRotationRight;
        private Sequence currentSequence;

        private void Start()
        {
            playerManager = FindFirstObjectByType<PlayerManager>();

            closeRotationLeft = doorLeft.localRotation;
            closeRotationRight = doorRight.localRotation;

            openRotationLeft = closeRotationLeft * Quaternion.AngleAxis(-openAngle, doorLeft.InverseTransformDirection(Vector3.up));
            openRotationRight = closeRotationRight * Quaternion.AngleAxis(openAngle, doorRight.InverseTransformDirection(Vector3.up));
        }

        private bool IsPlayerInFront()
        {
            if (playerManager == null || insidePoint == null || exitPoint == null) return false;

            Vector3 front = exitPoint.position - insidePoint.position;
            Vector3 toPlayer = playerManager.transform.position - insidePoint.position;
            front.y = 0f;
            toPlayer.y = 0f;

            return Vector3.Dot(front, toPlayer) > 0f;
        }

        public void Interact()
        {
            if (!CanInteract) return;

            occupied = true;
            busy = true;
            playerManager.EnterHide(this);

            currentSequence.Stop();

            currentSequence = Sequence.Create()
                .Group(Tween.LocalRotation(doorLeft, openRotationLeft, openDuration, doorEase))
                .Group(Tween.LocalRotation(doorRight, openRotationRight, openDuration, doorEase))
                
                .Chain(Tween.Position(playerManager.transform, insidePoint.position, movePlayerDuration, Ease.InOutQuad))
                .Group(Tween.Rotation(playerManager.transform, insidePoint.rotation, movePlayerDuration, Ease.InOutQuad))
                
                .Chain(Tween.LocalRotation(doorLeft, closeRotationLeft, closeDuration, Ease.OutQuad))
                .Group(Tween.LocalRotation(doorRight, closeRotationRight, closeDuration, Ease.OutQuad))
                
                .ChainCallback(target: this, target => target.busy = false);
            
        }

        public void RequestExit()
        {
            if (!occupied || busy) return;

            busy = true;
            currentSequence.Stop();


            currentSequence = Sequence.Create()
                .Group(Tween.LocalRotation(doorLeft, openRotationLeft, openDuration, doorEase))
                .Group(Tween.LocalRotation(doorRight, openRotationRight, openDuration, doorEase))

                .Chain(Tween.Position(playerManager.transform, exitPoint.position, movePlayerDuration, Ease.InOutQuad))
                
                .Chain(Tween.LocalRotation(doorLeft, closeRotationLeft, closeDuration, Ease.OutQuad))
                .Group(Tween.LocalRotation(doorRight, closeRotationRight, closeDuration, Ease.OutQuad))
                
                .ChainCallback(target: this, target =>
                {
                    target.busy = false;
                    target.occupied = false;
                    target.playerManager.LeaveHide();
                });
        }
    }
}
