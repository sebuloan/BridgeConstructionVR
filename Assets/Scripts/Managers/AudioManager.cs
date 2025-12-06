using UnityEngine;
using System;
using System.Collections.Generic;
using System.Collections;
//josn
public class AudioManager : MonoBehaviour
{
    public AudioSource stepDescriptionAudioSource;
    public AudioSource interactionAudioSource;
    public string audioPath;

    public void PlayStepDescriptionAudioClip(List<string> myAudioClip, Action callback)
    {
        StartCoroutine(PlayStepDescriptionAudioSequence(myAudioClip, callback));
    }
    public void PlayStepInteractionAudioclip(string clipName, Action callback)
    {
        StartCoroutine(PlayInteractionAudio(clipName, callback));
    }

    private IEnumerator PlayStepDescriptionAudioSequence(List<string> clipNames, Action callback)
    {
        foreach (string clipName in clipNames)
        {
            AudioClip clip = Resources.Load<AudioClip>(audioPath + clipName);
            if (clip != null)
            {
                stepDescriptionAudioSource.clip = clip;
                stepDescriptionAudioSource.Play();

                while (stepDescriptionAudioSource.isPlaying)
                {
                    yield return null;
                }
            }
            else
            {
                Debug.LogWarning("Audio clip not found: " + clipName);
            }
        }
        callback?.Invoke();
    }

   
    private IEnumerator PlayInteractionAudio(string clipName, Action callback)
    {
        AudioClip clip = Resources.Load<AudioClip>(audioPath + clipName);

        if (clip != null)
        {
            interactionAudioSource.clip = clip;
            interactionAudioSource.Play();
            while (interactionAudioSource.isPlaying)
            {
                yield return null;
            }
        }
        else
        {
            Debug.LogWarning("Audio clip not found: " + clipName);
        }

        callback?.Invoke();
    }
}
