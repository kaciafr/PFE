using UnityEngine;

namespace Goblfin.SoundSystem
{
    [System.Serializable]
    public class SoundItem
    {
        public SoundType type;
        public AudioClip[] clips;  
        [Range(0f, 1f)]
        public float volume = 1f;
        [Range(0.8f, 1.2f)]
        public float minPitch = 1f;
        [Range(0.8f, 1.2f)]
        public float maxPitch = 1f;
    }

    [System.Serializable]
    public class MusicItem
    {
        public MusicType type;
        public AudioClip clip;
        [Range(0f, 1f)]
        public float volume = 1f;
    }
}