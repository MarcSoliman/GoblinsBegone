using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FeedbackSpawner : MonoBehaviour
{
    static FeedbackSpawner instance;
    public static FeedbackSpawner Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<FeedbackSpawner>();
            }
            return instance;
        }
    }
    
    //function that spawns particle system
    public void SpawnParticleEffect(GameObject particleEffect, Vector3 position)
    {
        Instantiate(particleEffect, position, Quaternion.Euler(0, 0, 0));
     
    }
    
    //function that spawns audio clip
    public AudioSource PlayAudioClip2D(AudioClip clip, float volume, float minVolume, float maxVolume, float minPitch, float maxPitch)
    {
        // create
        GameObject audioObject = new GameObject("Audio2D");
        AudioSource audioSource = audioObject.AddComponent<AudioSource>();
        
        // configure
        audioSource.clip = clip;
        audioSource.volume = volume;
        
        //randomize volume
        audioSource.volume = Random.Range(minVolume, maxVolume);
        
        //randomize pitch
        audioSource.pitch = Random.Range(minPitch, maxPitch);
        
        //activate
        audioSource.Play();
        Object.Destroy(audioObject, clip.length);

        //return in case other things need it
        return audioSource;
    }

    internal void PlayAudioClip2D(object deathAudio, float v1, float v2, float v3, float v4, float v5)
    {
        throw new System.NotImplementedException();
    }
}
