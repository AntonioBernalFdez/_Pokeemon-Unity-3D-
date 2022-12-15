using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class SoundManager : MonoBehaviour
{
    [SerializeField] private AudioSource effectsSource, musicSource;

    public Vector2 pitchRange = Vector2.zero;
    public static SoundManager SharedInstance;

    private void Awake()
    {
        if (SharedInstance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            SharedInstance = this;
        }
        DontDestroyOnLoad(gameObject);
    }
    public void PlaySound(AudioClip clip)
    {
        effectsSource.pitch = 1;
        effectsSource.Stop();
        effectsSource.clip = clip;
        effectsSource.Play();
    }
    public void PlayMusic(AudioClip clip)
    {
        musicSource.Stop();
        musicSource.clip = clip;
        musicSource.Play();
    }
}
