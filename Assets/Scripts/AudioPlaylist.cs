using UnityEngine;
using System.Collections.Generic;

public class AudioPlaylist : MonoBehaviour
{
    public List<AudioClip> audioClips = new List<AudioClip>();

    [HideInInspector] public bool playing = false;
        
    AudioSource audioSource;
    int currentTrackIndex = -1;    

    void Awake()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 1.0f;
        audioSource.rolloffMode = AudioRolloffMode.Logarithmic;
        audioSource.minDistance = 3f;
    }

    void Update()
    {
        if (audioSource.isPlaying == false && audioClips.Count > 0 && playing)
        {
            PlayNextTrack();
        }
    }

    public void PlayNextTrack()
    {        
        currentTrackIndex = (currentTrackIndex + 1) % audioClips.Count;
        AudioClip nextClip = audioClips[currentTrackIndex];        
        audioSource.clip = nextClip;
        audioSource.Play();
    }
}