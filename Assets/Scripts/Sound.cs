using System;
using System.Collections;
using System.Collections.Generic;
using Main;
using UnityEngine;
using UnityEngine.Rendering;

[Serializable]
public class Sound : MonoBehaviour
{
    public static Sound Instance;
    [SerializeField] private AudioSource musicSource, effectSource;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        VolumeControl(SettingsPrefs.GetUpdatePrefs(SettingsPrefs.Sound));
    }

    public void VolumeControl(int volume)
    {
        AudioListener.volume = volume;
    }

    public void PlaySound(AudioClip clip)
    {
        effectSource.PlayOneShot(clip);
    }

    public void PlayMusic(AudioClip clip)
    {
        musicSource.clip = clip;
        musicSource.Play();
    }
    
    public void StopMusic()
    {
        musicSource.clip = null;
        musicSource.Stop();
    }
}
