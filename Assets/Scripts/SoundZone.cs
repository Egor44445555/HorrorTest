using UnityEngine;

public class SoundZone : MonoBehaviour
{
    public float fadeSpeed = 2f;

    float targetVolume = 0f;
    AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            targetVolume = 1f;
            
            if (!audioSource.isPlaying)
            {
                audioSource.Play();
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            targetVolume = 0f;
        }
    }

    void Update()
    {
        audioSource.volume = Mathf.Lerp(audioSource.volume, targetVolume, fadeSpeed * Time.deltaTime);

        if (audioSource.volume < 0.01f && audioSource.isPlaying)
        {
            audioSource.Stop();
        }
    }
}