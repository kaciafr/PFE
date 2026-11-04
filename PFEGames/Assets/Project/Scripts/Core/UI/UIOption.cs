using System;
using System.Collections.Generic;
using Goblfin.SoundSystem;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Goblfin
{
    public class UIOption :  UIScreen
    {
        
       [SerializeField] private Slider sliderMusic;
       [SerializeField] private Slider sliderSFX;
       [SerializeField] private Toggle FullScreen; 
       [SerializeField] private Button btnBack;
       [SerializeField] private TMP_Dropdown dropdownQuality;
       [SerializeField] private Button btnControls;
       [SerializeField] private UIScreen controlsScreen;



        private void Start()
        {
            btnControls.onClick.AddListener(() => UIManager.Instance.Push(controlsScreen));

            // Qualité EN PREMIER : elle ne dépend de rien d'autre
            dropdownQuality.ClearOptions();
            dropdownQuality.AddOptions(new List<string>(QualitySettings.names));

            // Clamp : si un niveau a été supprimé dans Project Settings, l'index sauvegardé peut être trop grand
            int quality = PlayerPrefs.GetInt("quality", QualitySettings.GetQualityLevel());
            quality = Mathf.Clamp(quality, 0, QualitySettings.names.Length - 1);
            dropdownQuality.SetValueWithoutNotify(quality);
            dropdownQuality.RefreshShownValue();
            QualitySettings.SetQualityLevel(quality);

            FullScreen.isOn = Screen.fullScreen;

            float music = PlayerPrefs.GetFloat("MusicVolume", 1f);
            float sfx = PlayerPrefs.GetFloat("SFXVolume", 1f);

            sliderMusic.value = music;
            sliderSFX.value = sfx;

            // Pas de SoundManager dans la scène → on ne plante pas, on prévient
            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.SetMusicVolume(music);
                SoundManager.Instance.SetSFXVolume(sfx);
            }
            else
            {
                Debug.LogWarning("UIOption : pas de SoundManager dans la scène, les volumes ne s'appliquent pas.");
            }

            sliderMusic.onValueChanged.AddListener(SetMusic);
            sliderSFX.onValueChanged.AddListener(SetSFX);
            FullScreen.onValueChanged.AddListener(SetFullscreen);
            dropdownQuality.onValueChanged.AddListener(SetQuality);
            
            btnBack.onClick.AddListener(UIManager.Instance.Pop);




        }

        private void SetMusic(float value)
        {
            if (SoundManager.Instance != null) SoundManager.Instance.SetMusicVolume(value);
            PlayerPrefs.SetFloat("MusicVolume", value);
        }

        private void SetSFX(float value)
        {
            if (SoundManager.Instance != null) SoundManager.Instance.SetSFXVolume(value);
            PlayerPrefs.SetFloat("SFXVolume", value);
        }

        private void SetFullscreen(bool value)
        {
            Screen.fullScreen = value;
        }

        private void SetQuality(int index)
        {
            QualitySettings.SetQualityLevel(index);
            PlayerPrefs.SetInt("quality", index);

        }
    }
}