using UnityEngine;

namespace Goblfin.PnjSystem
{
    public class SoundImpactFall : MonoBehaviour
    {
        [SerializeField] private AudioSource impactSound;
        [SerializeField] private FallingItems items;

        private void OnEnable()
        {
            items.FallImpact += Impact;
        }
        private void OnDisable()
        {
            items.FallImpact -= Impact;
        }

        private void Impact()
        {
            impactSound.Play();
        }
    }
}