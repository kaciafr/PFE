using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Goblfin.SoundSystem
{
    public class SoundManager : MonoBehaviour
    {
        public static SoundManager Instance;
        
        [Header(" Volumes")]
        [Range(0f, 1f)] public float masterVolume = 1f;
        [Range(0f, 1f)] public float musicVolume = 1f;
        [Range(0f, 1f)] public float sfxVolume = 1f;


        [Header("Sources Audio")]
        [SerializeField] private AudioSource musicSource;
        [SerializeField] private AudioSource sfxSource;


        [Header(" Tous les SFX")]
        [SerializeField] private SoundItem[] sounds;

        [Header(" Toutes les Musiques")]
        [SerializeField] private MusicItem[] musics;


        private Dictionary<SoundType, SoundItem> soundDict;
        private Dictionary<MusicType, MusicItem> musicDict;
        
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }
            
            InitDictionaries();
        }
        private void InitDictionaries()
        {

            soundDict = new Dictionary<SoundType, SoundItem>();
            foreach (SoundItem sound in sounds)
            {
                if (!soundDict.ContainsKey(sound.type))
                {
                    soundDict.Add(sound.type, sound);
                }
                else
                {
                    Debug.LogWarning("SoundType en double : " + sound.type);
                }
            }

            musicDict = new Dictionary<MusicType, MusicItem>();
            foreach (MusicItem music in musics)
            {
                if (!musicDict.ContainsKey(music.type))
                {
                    musicDict.Add(music.type, music);
                }
                else
                {
                    Debug.LogWarning("MusicType en double : " + music.type);
                }
            }
        }

   
        public void PlaySFX(SoundType type , float volumeScale = 1f)
        {
            if (!soundDict.ContainsKey(type))
            {
                Debug.LogWarning("Son pas trouvé : " + type);
                return;
            }

            SoundItem sound = soundDict[type];

            if (sound.clips == null || sound.clips.Length == 0) return;

            AudioClip clip = sound.clips[Random.Range(0, sound.clips.Length)];

            sfxSource.pitch = Random.Range(sound.minPitch, sound.maxPitch);

            sfxSource.PlayOneShot(clip, sound.volume * sfxVolume * masterVolume * volumeScale);
        }


        public void PlayMusic(MusicType type)
        {
            if (!musicDict.ContainsKey(type))
            {
                Debug.LogWarning("Musique pas trouvée : " + type);
                return;
            }

            MusicItem music = musicDict[type];

            if (musicSource.clip == music.clip) return;

            musicSource.clip = music.clip;
            musicSource.volume = music.volume * musicVolume * masterVolume;
            musicSource.loop = true;
            musicSource.Play();
        }

        public void PlayMusicWithFade(MusicType type, float fadeDuration = 1f)
        {
            if (!musicDict.ContainsKey(type)) return;

            MusicItem music = musicDict[type];

            if (musicSource.clip == music.clip) return;

            StartCoroutine(FadeMusic(music, fadeDuration));
        }

        private IEnumerator FadeMusic(MusicItem newMusic, float duration)
        {
            float startVolume = musicSource.volume;

            while (musicSource.volume > 0f)
            {
                musicSource.volume -= startVolume * Time.deltaTime / duration;
                yield return null;
            }

            musicSource.Stop();
            musicSource.clip = newMusic.clip;
            musicSource.Play();

            float targetVolume = newMusic.volume * musicVolume * masterVolume;
            while (musicSource.volume < targetVolume)
            {
                musicSource.volume += targetVolume * Time.deltaTime / duration;
                yield return null;
            }

            musicSource.volume = targetVolume;
        }

  

        private IEnumerator FadeMusicOut(float duration)
        {
            float startVolume = musicSource.volume;
            while (musicSource.volume > 0f)
            {
                musicSource.volume -= startVolume * Time.deltaTime / duration;
                yield return null;
            }
            musicSource.Stop();
            musicSource.clip = null;
        }
    
        public void SetMasterVolume(float value)
        {
            masterVolume = value;
            musicSource.volume = musicVolume * masterVolume;
        }

        public void SetMusicVolume(float value)
        {
            musicVolume = value;
            musicSource.volume = musicVolume * masterVolume;
        }

        public void SetSFXVolume(float value)
        {
            sfxVolume = value;
        }
    
        public void StopMusic(float fadeDuration = 1f)
        {
            StopAllCoroutines(); 

            if (fadeDuration <= 0f)
            {
                musicSource.Stop();
                musicSource.clip = null;
                return;
            }

            StartCoroutine(FadeMusicOut(fadeDuration));
        }
        
        public void StopSFX()
        {
            sfxSource.Stop();
            sfxSource.clip = null;
        }
        
        public void PlayDialogueLoop(SoundType type)
        {
            if (!soundDict.ContainsKey(type)) return;
            SoundItem sound = soundDict[type];
            if (sound.clips == null || sound.clips.Length == 0) return;
            AudioClip clip = sound.clips[Random.Range(0, sound.clips.Length)];
            sfxSource.clip   = clip;
            sfxSource.loop   = true;
            sfxSource.volume = sound.volume * sfxVolume * masterVolume;
            sfxSource.Play();
        }

        public void StopDialogueLoop()
        {
            sfxSource.loop = false;
            sfxSource.Stop();
        }
    }
}