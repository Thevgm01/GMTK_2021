using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class AudioHelper
{
    public static AudioSource PlayRandomClip2DFromArray(AudioClip[] clips, float volume = 1f, float pitch = 1f, bool destroyWhenDone = true)
    {
        return PlayClip2D(clips[UnityEngine.Random.Range(0, clips.Length)], volume, pitch, destroyWhenDone);
    }

    public static AudioSource PlayClip2D(AudioClip clip, float volume = 1f, float pitch = 1f, bool destroyWhenDone = true)
    {
        GameObject audioObject = new GameObject("Audio2D");
        AudioSource audioSource = audioObject.AddComponent<AudioSource>();
        audioSource.clip = clip;
        audioSource.volume = volume;
        audioSource.pitch = pitch;
        audioSource.Play();
        if(destroyWhenDone) Object.Destroy(audioObject, clip.length);
        return audioSource;
    }
}
