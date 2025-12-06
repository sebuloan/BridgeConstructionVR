using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ParticleSystemManager : MonoBehaviour
{
    private ParticleSystem ParticlePlayer;

    public void Initialize(string particleObjectName, Action callback)
    {
        GameObject particleObject = GameObject.Find(particleObjectName);

        if (particleObject == null)
        {
            Debug.LogError($"ParticlePlayer: No GameObject found with name '{particleObjectName}'.");
            return;
        }

        ParticlePlayer = particleObject.GetComponent<ParticleSystem>();
        if (ParticlePlayer == null)
        {
            Debug.LogError("ParticlePlayer: No ParticleSystem component found on the GameObject.");
            return;
        }

        Play(callback);
    }

    public void Play(Action callback)
    {
        if (ParticlePlayer == null)
        {
            Debug.LogWarning("ParticlePlayer: Particle system is not initialized.");
            return;
        }

        ParticlePlayer.gameObject.SetActive(true); // Ensure it's active before playing
        ParticlePlayer.Play();
        StartCoroutine(CheckParticleEnd(callback));
    }

    private IEnumerator CheckParticleEnd(Action callback)
    {
        while (ParticlePlayer != null && ParticlePlayer.isPlaying)
        {
            yield return null;
        }

        ParticlePlayer.Stop();
        ParticlePlayer.gameObject.SetActive(false); // Disable GameObject
        callback?.Invoke();
    }
}
