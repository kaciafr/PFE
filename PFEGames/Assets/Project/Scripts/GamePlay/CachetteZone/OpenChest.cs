using Goblfin.CharactersSystem.Player.Data;
using Goblfin.CharactersSystem.Player.Interface;
using Goblfin.Player;
using PrimeTween;
using UnityEngine;

namespace Goblfin.Gameplay.CachetteZone
{
    public class OpenChest : MonoBehaviour , IInteractable, IHideSpot
    {
        [Header("Pièces du coffre")]
        [SerializeField] private Transform lidChest;
        [SerializeField] private Transform lowerChest;
        [SerializeField, Tooltip("Où le gobelin atterrit dans le coffre.")] private Transform insidePoint;
        [SerializeField, Tooltip("Où le gobelin atterrit en sortant.")] private Transform exitPoint;

        [Header("Couvercle")]
        [SerializeField] private float rotation = 110f;
        [SerializeField] private float shakeDuration = 0.3f;
        [SerializeField] private float openDuration = 0.4f;
        [SerializeField] private float closeDuration = 0.25f;
        [SerializeField] private float popStrength = 0.15f;
        [SerializeField] private float popDuration = 0.3f;

        [Header("Saut")]
        [SerializeField] private float jumpHeight = 1f;
        [SerializeField] private float jumpDuration = 0.6f;
        [SerializeField, Tooltip("Temps entre le début de l'animation de saut et le décollage du sol.")]
        private float takeoffDelay = 0.25f;

        private PlayerManager player;
        private Quaternion closedRotation;
        private Quaternion openRotation;
        private bool occupied;
        private bool busy;

        public bool CanInteract => !occupied && !busy;

        private void Start()
        {
            player = FindFirstObjectByType<PlayerManager>();
            closedRotation = lidChest.localRotation;
            openRotation = closedRotation * Quaternion.Euler(0f, 0f, rotation);
        }

        public void Interact()
        {
            occupied = true;
            busy = true;
            player.EnterHide(this);

            Sequence.Create()
                .Chain(Tween.ShakeLocalRotation(lidChest, new Vector3(4f, 0f, 2f), shakeDuration, frequency: 25))
                .ChainCallback(() => player.Animator.Play(AnimIds.Jump, 0.05f))
                .Chain(OpenLid())
                .Group(Squash())
                .Group(Pop())
                .Group(JumpArc(player.transform.position, insidePoint.position))
                .ChainCallback(() => player.SetVisible(false))
                .Chain(CloseLid())
                .Group(Pop())
                .ChainCallback(() => busy = false);
        }

        public void RequestExit()
        {
            if (busy) return;
            busy = true;

            Sequence.Create()
                .ChainCallback(() =>
                {
                    player.SetVisible(true);
                    player.Animator.Play(AnimIds.Jump, 0.05f);
                })
                .Chain(OpenLid())
                .Group(Pop())
                .Group(JumpArc(insidePoint.position, exitPoint.position))
                .ChainCallback(() => player.Animator.Play(AnimIds.Land, 0.05f))
                .Chain(CloseLid())
                .ChainCallback(() =>
                {
                    occupied = false;
                    busy = false;
                    player.LeaveHide();
                });
        }

        private Tween OpenLid() => Tween.LocalRotation(lidChest, openRotation, openDuration, Ease.OutBack);
        private Tween CloseLid() => Tween.LocalRotation(lidChest, closedRotation, closeDuration, Ease.InQuad);
        private Tween Pop() => Tween.PunchScale(transform, Vector3.one * popStrength, popDuration, frequency: 4);
        private Tween Squash() => Tween.PunchScale(lowerChest, new Vector3(0.15f, -0.2f, 0.15f), 0.35f, frequency: 6);

        private Tween JumpArc(Vector3 from, Vector3 to) =>
            Tween.Custom(0f, 1f, jumpDuration, t =>
            {
                Vector3 ground = Vector3.Lerp(from, to, Mathf.SmoothStep(0f, 1f, t));
                float height = 4f * t * (1f - t) * jumpHeight;
                player.transform.position = ground + Vector3.up * height;
            }, Ease.Linear, startDelay: takeoffDelay);
    }
}
