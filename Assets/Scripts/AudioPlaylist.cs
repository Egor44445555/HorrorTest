using UnityEngine;
using System.Collections.Generic;

public class AudioPlaylist : MonoBehaviour
{
    public List<AudioClip> audioClips = new List<AudioClip>();
    
    [HideInInspector] public bool playing = false;
    [SerializeField] float delayBetweenTracks = 0.1f;
    
    AudioSource audioSource;
    int currentTrackIndex = -1;
    float trackEndTime = 0f;
    bool waitingForNextTrack = false;

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
        if (UIManager.main.gamePause) 
        {
            if (audioSource.isPlaying) audioSource.Pause();

            return;
        }
        else if (!audioSource.isPlaying && audioSource.time > 0)
        {
            audioSource.UnPause();
        }
        
        if (playing && audioClips.Count > 0)
        {
            if (audioSource.isPlaying == false && !waitingForNextTrack)
            {
                trackEndTime = Time.time + delayBetweenTracks;
                waitingForNextTrack = true;
            }
            
            if (waitingForNextTrack && Time.time >= trackEndTime)
            {
                waitingForNextTrack = false;
                PlayNextTrack();
            }
        }
    }

    public void PlayNextTrack()
    {
        if (audioClips.Count == 0) 
        {
            playing = false;
            return;
        }
        
        currentTrackIndex = (currentTrackIndex + 1) % audioClips.Count;
        AudioClip nextClip = audioClips[currentTrackIndex];
        
        if (nextClip != null)
        {
            audioSource.clip = nextClip;
            audioSource.Play();
        }
        else
        {
            PlayNextTrack();
        }
    }
}