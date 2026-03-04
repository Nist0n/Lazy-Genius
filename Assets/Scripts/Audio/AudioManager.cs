using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using Random = UnityEngine.Random;

namespace Audio
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance;

        [SerializeField] private AudioSource musicSource;
        [SerializeField] private AudioSource sfxSource;
        [SerializeField] private AudioResource musicAudioRandomController;
        [SerializeField] private List<Sound> music, sounds;
        [SerializeField] private AudioMixer audioMixer;

        private void Awake()
        {
            if (!Instance)
            {
                Instance = this;
                if (PlayerPrefs.HasKey("musicVolume") && 
                    PlayerPrefs.HasKey("SFXVolume") && 
                    PlayerPrefs.HasKey("enemiesVolume") && 
                    PlayerPrefs.HasKey("environmentVolume") && 
                    PlayerPrefs.HasKey("effectsVolume"))
                {
                    LoadVolume();
                }
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        public void PlayMusic(string soundName)
        {
            Sound s = music.Find(x => x.name == soundName);

            if (s == null)
            {
                Debug.LogWarning("Music: " + soundName + " not found!");
                return;
            }
            
            musicSource.clip = s.audio;
            musicSource.Play();
        }
        
        public void PlaySFX(string soundName)
        {
            Sound s = sounds.Find(x => x.name == soundName);

            if (s == null)
            {
                Debug.LogWarning("Sound " + soundName + " not found!");
                return;
            }
            
            sfxSource.PlayOneShot(s.audio);
        }

        public void PlayRandomSoundByName(string soundName, AudioSource source)
        {
            List<Sound> matchingSounds = sounds.FindAll(sound => sound.name == soundName);

            if (matchingSounds.Count > 0)
            {
                int randomIndex = Random.Range(0, matchingSounds.Count);
                Sound randomSound = matchingSounds[randomIndex];

                source.PlayOneShot(randomSound.audio);
            }
        }

        public void PlayWalkSound(string soundName, AudioSource source)
        {
            List<Sound> matchingSounds = sounds.FindAll(sound => sound.name == soundName);

            if (matchingSounds.Count > 0)
            {
                int randomIndex = Random.Range(0, matchingSounds.Count);
                Sound randomSound = matchingSounds[randomIndex];

                source.clip = randomSound.audio;
                source.Play();
            }
        }

        public void PlayLocalSound(string soundName,AudioSource source)
        {
            Sound s = sounds.Find(sound => sound.name == soundName);

            if (s == null)
            {
                Debug.LogWarning("Sound " + soundName + " not found!");
                return;
            }
            
            source.PlayOneShot(s.audio);
        }

        public void StopMusic()
        {
            musicSource.Stop();
        }

        public void StartMusicShuffle()
        {
            musicSource.resource = musicAudioRandomController;
            musicSource.Play();
        }

        public void StopMusicSourceLoop()
        {
            musicSource.loop = false;
        }
        
        private void LoadVolume()
        {
            float musicVolume = PlayerPrefs.GetFloat("musicVolume");
            audioMixer.SetFloat("music", MathF.Log10(musicVolume) * 20);

            float sfxVolume = PlayerPrefs.GetFloat("SFXVolume");
            audioMixer.SetFloat("sfx", MathF.Log10(sfxVolume) * 20);
            
            float environmentVolume = PlayerPrefs.GetFloat("environmentVolume");
            audioMixer.SetFloat("environment", MathF.Log10(environmentVolume) * 20);
            
            float effectsVolume = PlayerPrefs.GetFloat("effectsVolume");
            audioMixer.SetFloat("effects", MathF.Log10(effectsVolume) * 20);
            
            float enemiesVolume = PlayerPrefs.GetFloat("enemiesVolume");
            audioMixer.SetFloat("enemies", MathF.Log10(enemiesVolume) * 20);
        }
    }
}
