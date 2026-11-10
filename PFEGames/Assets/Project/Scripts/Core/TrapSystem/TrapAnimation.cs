using System;
using System.Collections.Generic;
using PrimeTween;
using UnityEngine;

namespace Goblfin.TrapSystem
{
    public class TrapAnimation : MonoBehaviour
    {
        [Serializable]
        public class WhatIsMoving
        {
            public GameObject objet;

            [Header("Rotation (local, euler) - mettre -90 et -160")]
            public Vector3 rotationClosed;

            [HideInInspector] public Quaternion openRot;
            [HideInInspector] public Quaternion closedRot;
        }

        [SerializeField] private TrapSysteme trapSysteme;
        [SerializeField] private List<WhatIsMoving> partOfTrap;
        [SerializeField] private AudioSource soundHit;

        [Header("Fermeture (snap)")]
        [SerializeField] private float closeDuration = 0.1f;
        [SerializeField] private Ease closeEase = Ease.InQuad;
        [SerializeField] private float shakeDuration = 0.2f;
        [SerializeField] private float shakeStrength = 0.05f;

        [Header("Ouverture (reset)")]
        [SerializeField] private float openDuration = 0.8f;
        [SerializeField] private Ease openEase = Ease.OutQuad;

        private Sequence currentSequence;

        private void Start()
        {
            foreach (var part in partOfTrap)
            {
                part.openRot = part.objet.transform.localRotation;
                part.closedRot = Quaternion.Euler(part.rotationClosed);
                part.objet.transform.localRotation = part.openRot;
            }
        }

        private void OnEnable()
        {
            trapSysteme.TrapTriggered += TrapClosed;
            trapSysteme.TrapOpen += TrapOpen;
        }

        private void OnDisable()
        {
            trapSysteme.TrapTriggered -= TrapClosed;
            trapSysteme.TrapOpen -= TrapOpen;
            currentSequence.Stop();
        }

        private void TrapClosed()
        {
            currentSequence.Stop();
            soundHit.Play();
            currentSequence = Sequence.Create();

            var closeSettings = new TweenSettings(closeDuration, closeEase);

            foreach (var part in partOfTrap)
            {
                var t = part.objet.transform;
                currentSequence.Group(
                    Tween.LocalRotation(t, t.localRotation, part.closedRot, closeSettings));
            }

            currentSequence.Chain(Tween.ShakeLocalPosition(
                transform,
                new ShakeSettings
                {
                    strength = Vector3.one * shakeStrength,
                    duration = shakeDuration,
                    frequency = 25
                }));
        }

        private void TrapOpen()
        {
            currentSequence.Stop();
            currentSequence = Sequence.Create();

            var openSettings = new TweenSettings(openDuration, openEase);

            foreach (var part in partOfTrap)
            {
                var t = part.objet.transform;
                currentSequence.Group(
                    Tween.LocalRotation(t, t.localRotation, part.openRot, openSettings));
            }
        }
    }
}