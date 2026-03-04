using System;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

namespace Audio
{
    public class VolumeSettings : MonoBehaviour
    {
        [SerializeField] private AudioMixer audioMixer;
        [SerializeField] private Slider musicSlider;
        [SerializeField] private Slider SFXSlider;
        [SerializeField] private Slider environmentSlider;
        [SerializeField] private Slider effectsSlider;
        [SerializeField] private Slider enemiesSlider;

        private void Start()
        {
            if (PlayerPrefs.HasKey("musicVolume") && 
                PlayerPrefs.HasKey("SFXVolume") && 
                PlayerPrefs.HasKey("enemiesVolume") && 
                PlayerPrefs.HasKey("environmentVolume") && 
                PlayerPrefs.HasKey("effectsVolume"))
            {
                LoadVolume();
            }
            else
            {
                SetMusicVolume();
                SetSFXVolume();
                SetEnvironmentVolume();
                SetEffectsVolume();
                SetEnemiesVolume();
            }
        }

        public void SetMusicVolume()
        {
            float volume = musicSlider.value;
            audioMixer.SetFloat("music", MathF.Log10(volume) * 20);
            PlayerPrefs.SetFloat("musicVolume", volume);
        }

        public void SetSFXVolume()
        {
            float volume = SFXSlider.value;
            audioMixer.SetFloat("sfx", MathF.Log10(volume) * 20);
            PlayerPrefs.SetFloat("SFXVolume", volume);
        }
        
        public void SetEnemiesVolume()
        {
            float volume = enemiesSlider.value;
            audioMixer.SetFloat("enemies", MathF.Log10(volume) * 20);
            PlayerPrefs.SetFloat("enemiesVolume", volume);
        }
        
        public void SetEnvironmentVolume()
        {
            float volume = environmentSlider.value;
            audioMixer.SetFloat("environment", MathF.Log10(volume) * 20);
            PlayerPrefs.SetFloat("environmentVolume", volume);
        }
        
        public void SetEffectsVolume()
        {
            float volume = effectsSlider.value;
            audioMixer.SetFloat("effects", MathF.Log10(volume) * 20);
            PlayerPrefs.SetFloat("effectsVolume", volume);
        }

        private void LoadVolume()
        {
            float musicVolume = PlayerPrefs.GetFloat("musicVolume");
            musicSlider.value = musicVolume;

            float sfxVolume = PlayerPrefs.GetFloat("SFXVolume");
            SFXSlider.value = sfxVolume;
            
            float environmentVolume = PlayerPrefs.GetFloat("environmentVolume");
            environmentSlider.value = environmentVolume;
            
            float effectsVolume = PlayerPrefs.GetFloat("effectsVolume");
            effectsSlider.value = effectsVolume;
            
            float enemiesVolume = PlayerPrefs.GetFloat("enemiesVolume");
            enemiesSlider.value = enemiesVolume;
        }
    }
}
