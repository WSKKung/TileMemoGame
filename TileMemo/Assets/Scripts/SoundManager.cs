using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public AudioSource songAudioSource;
    public AudioSource soundAudioSource;

    Dictionary<string, AudioClip> audioClips = new Dictionary<string, AudioClip>();
   

    //Singleton instance
    public static SoundManager Instance = null;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else if (Instance != this) Destroy(gameObject);

        foreach (var eachLoadedClip in Resources.LoadAll<AudioClip>("Audio/"))
        {
            audioClips.Add(eachLoadedClip.name, eachLoadedClip);
        }

        DontDestroyOnLoad(gameObject);
    }

    public void Play(string clipName)
    {
        if (soundAudioSource.isPlaying) soundAudioSource.Stop();
        soundAudioSource.clip = audioClips[clipName];
        soundAudioSource.Play();
    }
    public void PlayMusic(string clipName, float volume=1f, bool isLoop = false)
    {
        StopCurrentMusic();
        songAudioSource.volume = volume;
        songAudioSource.clip = audioClips[clipName];
        songAudioSource.Play();
        songAudioSource.loop = isLoop;
    }

    //load audio clip from Resources/Audio folder into the SoundManager
    public void LoadAudioClip(string audioName)
    {
        var clip = Resources.Load<AudioClip>("Audio/" + audioName);
        if (clip != null) audioClips.Add(audioName, clip);
    }

    public void StopCurrentMusic()
    {
        if (songAudioSource.isPlaying) songAudioSource.Stop();
    }
}
